using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Dbml;

/// <summary>
/// A formatter for database tables to create DBML files.
/// </summary>
/// <seealso cref="IDbmlFormatter" />
public class DbmlFormatter : IDbmlFormatter
{
    /// <summary>
    /// Renders database tables as a DBML format.
    /// </summary>
    /// <param name="tables">A collection of database tables.</param>
    /// <returns>A string, in DBML format.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" /> or has <see langword="null" /> values.</exception>
    public string RenderTables(IReadOnlyCollection<IRelationalDatabaseTable> tables)
    {
        if (tables.NullOrAnyNull())
            throw new ArgumentNullException(nameof(tables));

        if (tables.Count == 0)
            return string.Empty;

        var builder = StringBuilderCache.Acquire();

        var hasFirstTable = false;
        foreach (var table in tables)
        {
            if (hasFirstTable)
                builder.AppendLine();

            RenderTable(builder, table);

            hasFirstTable = true;
        }

        var renderedTableNames = new HashSet<Identifier>(tables.Select(static t => t.Name), IdentifierComparer.Ordinal);

        var hasFirstForeignKey = false;
        foreach (var table in tables)
        {
            var parentKeys = GetRenderableParentKeys(table, renderedTableNames);
            if (parentKeys.Count == 0)
                continue;

            if (!hasFirstForeignKey)
            {
                builder.AppendLine();
                hasFirstForeignKey = true;
            }

            RenderForeignKeys(builder, table, parentKeys);
        }

        RenderTableGroups(builder, tables);

        return builder.GetStringAndRelease().TrimEnd();
    }

    /// <summary>
    /// Emits one <c>TableGroup</c> per schema, so that a diagram lays the tables of a schema out
    /// together. A table whose name carries no schema cannot be placed in a group, and a database
    /// whose tables all share one schema gains nothing from grouping, so neither is rendered.
    /// </summary>
    private static void RenderTableGroups(StringBuilder builder, IReadOnlyCollection<IRelationalDatabaseTable> tables)
    {
        var tablesBySchema = tables
            .Select(static t => new { Table = t, Qualifier = t.Name.ToDbmlQualifier() })
            .Where(static t => t.Qualifier != null)
            .GroupBy(static t => t.Qualifier!, StringComparer.Ordinal)
            .OrderBy(static g => g.Key, StringComparer.Ordinal)
            .ToList();

        if (tablesBySchema.Count < 2)
            return;

        foreach (var schemaTables in tablesBySchema)
        {
            builder.AppendLine();
            builder.Append("TableGroup ")
                .Append(schemaTables.Key.ToDbmlIdentifier())
                .AppendLine(" {");

            foreach (var table in schemaTables)
            {
                builder.Append(Indent)
                    .AppendLine(table.Table.Name.ToDbmlName());
            }

            builder.AppendLine("}");
        }
    }

    private static void RenderTable(StringBuilder builder, IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(table);

        var tableName = table.Name.ToDbmlName();
        builder.Append("Table ")
            .Append(tableName)
            .AppendLine(" {");

        foreach (var column in table.Columns)
            RenderColumnLine(builder, table, column);

        RenderIndexes(builder, table);

        builder.AppendLine("}");
    }

    private static void RenderColumnLine(StringBuilder builder, IRelationalDatabaseTable table, IDatabaseColumn column)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(column);

        var typeName = column.Type.Definition.RemoveEnclosingQuotingCharacters().ToDbmlTypeName();

        builder.Append(Indent)
            .Append(column.Name.ToDbmlLocalName())
            .Append(' ')
            .Append(typeName)
            .Append(" [")
            .Append(column.IsNullable ? "null" : "not null");

        if (column.AutoIncrement.IsSome)
            builder.Append(", increment");

        if (ColumnIsPrimaryKey(table, column))
            builder.Append(", primary key");
        else if (ColumnIsUniqueKey(table, column))
            builder.Append(", unique");

        column.Default.IfSome(def => builder.Append(", default: ").Append(def.ToDbmlDefaultValue()));

        // DBML has no computed column syntax, so the expression is preserved as a column note
        if (column.IsComputed)
            builder.Append(", note: ").Append(BuildComputedColumnNote(column).ToDbmlStringLiteral());

        builder.AppendLine("]");
    }

    private static string BuildComputedColumnNote(IDatabaseColumn column)
    {
        var description = column.ComputedStorage switch
        {
            ComputedColumnStorage.Stored => "stored computed column",
            ComputedColumnStorage.Virtual => "virtual computed column",
            _ => "computed column"
        };

        return column.ComputedDefinition.Match(def => description + ": " + def, () => description);
    }

    private static void RenderIndexes(StringBuilder builder, IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(table);

        var compositeKeys = new List<IDatabaseKey>();
        table.PrimaryKey.Filter(static pk => pk.Columns.Count > 1).IfSome(compositeKeys.Add);
        var compositePrimaryKeyCount = compositeKeys.Count;
        compositeKeys.AddRange(table.UniqueKeys.Where(static uk => uk.Columns.Count > 1));

        var renderableIndexes = table.Indexes
            .Where(index => !index.IsUnique || !compositeKeys.Exists(key => IsIndexForKey(index, key)))
            .ToList();

        if (compositeKeys.Count == 0 && renderableIndexes.Count == 0)
            return;

        builder.AppendLine()
            .Append(Indent)
            .AppendLine("Indexes {");

        for (var i = 0; i < compositeKeys.Count; i++)
            RenderKeyIndexLine(builder, compositeKeys[i], i < compositePrimaryKeyCount ? "pk" : "unique");

        foreach (var index in renderableIndexes)
            RenderIndexLine(builder, index);

        builder.Append(Indent)
            .AppendLine("}");
    }

    private static void RenderKeyIndexLine(StringBuilder builder, IDatabaseKey key, string keyOption)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(keyOption);

        builder.Append(Indent)
            .Append(Indent)
            .Append('(')
            .Append(key.Columns.Select(static c => c.Name.ToDbmlLocalName()).Join(", "))
            .Append(") [");

        key.Name.IfSome(name => builder.Append("name: ").Append(name.ToVisibleName().ToDbmlStringLiteral()).Append(", "));

        builder.Append(keyOption)
            .AppendLine("]");
    }

    private static bool IsIndexForKey(IDatabaseIndex index, IDatabaseKey key)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(key);

        var keyColumnNames = key.Columns.Select(static c => c.Name.LocalName);
        return GetIndexColumnNames(index).SequenceEqual(keyColumnNames, StringComparer.Ordinal);
    }

    private static IEnumerable<string> GetIndexColumnNames(IDatabaseIndex index)
    {
        ArgumentNullException.ThrowIfNull(index);

        return index.Columns
            .Select(static ic => ic.Expression.RemoveEnclosingQuotingCharacters());
    }

    private static void RenderIndexLine(StringBuilder builder, IDatabaseIndex index)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(index);

        builder.Append(Indent)
            .Append(Indent);

        if (index.Columns.Count > 1)
        {
            builder.Append('(')
                .Append(index.Columns.Select(RenderIndexColumn).Join(", "))
                .Append(')');
        }
        else
        {
            builder.Append(RenderIndexColumn(index.Columns.Single()));
        }

        builder.Append(" [name: ")
            .Append(index.Name.ToVisibleName().ToDbmlStringLiteral());

        if (index.IsUnique)
            builder.Append(", unique");

        builder.AppendLine("]");
    }

    private static string RenderIndexColumn(IDatabaseIndexColumn indexColumn)
    {
        ArgumentNullException.ThrowIfNull(indexColumn);

        if (indexColumn.DependentColumns.Count == 1)
        {
            var columnName = indexColumn.DependentColumns[0].Name;
            var expression = indexColumn.Expression.RemoveEnclosingQuotingCharacters();
            if (string.Equals(expression, columnName.LocalName, StringComparison.Ordinal))
                return columnName.ToDbmlLocalName();
        }

        return indexColumn.Expression.ToDbmlExpression();
    }

    private static List<IDatabaseRelationalKey> GetRenderableParentKeys(IRelationalDatabaseTable table, IReadOnlySet<Identifier> renderedTableNames)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(renderedTableNames);

        return table.ParentKeys
            .Where(fk => renderedTableNames.Contains(fk.ParentTable))
            .ToList();
    }

    private static void RenderForeignKeys(StringBuilder builder, IRelationalDatabaseTable table, IEnumerable<IDatabaseRelationalKey> parentKeys)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(parentKeys);

        var childTableName = table.Name.ToDbmlName();
        var uniqueColumnSets = GetUniqueColumnSets(table);

        foreach (var relationalKey in parentKeys)
        {
            var isChildKeyUnique = IsChildKeyUnique(uniqueColumnSets, relationalKey.ChildKey);
            var relationalOperator = isChildKeyUnique ? '-' : '>';

            builder.Append("Ref: ")
                .Append(childTableName)
                .Append('.')
                .Append(RenderKeyColumns(relationalKey.ChildKey))
                .Append(' ')
                .Append(relationalOperator)
                .Append(' ')
                .Append(relationalKey.ParentTable.ToDbmlName())
                .Append('.')
                .AppendLine(RenderKeyColumns(relationalKey.ParentKey));
        }
    }

    private static string RenderKeyColumns(IDatabaseKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return key.Columns.Count > 1
            ? "(" + key.Columns.Select(static c => c.Name.ToDbmlLocalName()).Join(", ") + ")"
            : key.Columns.Single().Name.ToDbmlLocalName();
    }

    private static bool ColumnIsPrimaryKey(IRelationalDatabaseTable table, IDatabaseColumn column)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(column);

        return table.PrimaryKey
            .Match(
                pk => pk.Columns.Count == 1
                    && string.Equals(pk.Columns.First().Name.LocalName, column.Name.LocalName, StringComparison.Ordinal),
                static () => false
            );
    }

    private static bool ColumnIsUniqueKey(IRelationalDatabaseTable table, IDatabaseColumn column)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(column);

        return table.UniqueKeys
            .Any(
                uk => uk.Columns.Count == 1
                    && string.Equals(uk.Columns.First().Name.LocalName, column.Name.LocalName, StringComparison.Ordinal));
    }

    private static List<HashSet<string>> GetUniqueColumnSets(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var result = new List<HashSet<string>>(1 + table.UniqueKeys.Count + table.Indexes.Count);

        table.PrimaryKey.IfSome(pk => result.Add(GetKeyColumnNames(pk)));
        foreach (var uniqueKey in table.UniqueKeys)
            result.Add(GetKeyColumnNames(uniqueKey));
        foreach (var index in table.Indexes.Where(static i => i.IsUnique))
            result.Add(GetIndexColumnNames(index).ToHashSet(StringComparer.Ordinal));

        result.RemoveAll(static columnNames => columnNames.Count == 0);

        return result;
    }

    private static HashSet<string> GetKeyColumnNames(IDatabaseKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return key.Columns.Select(static c => c.Name.LocalName).ToHashSet(StringComparer.Ordinal);
    }

    private static bool IsChildKeyUnique(IEnumerable<HashSet<string>> uniqueColumnSets, IDatabaseKey key)
    {
        ArgumentNullException.ThrowIfNull(uniqueColumnSets);
        ArgumentNullException.ThrowIfNull(key);

        var keyColumnNames = GetKeyColumnNames(key);

        return uniqueColumnSets.Any(keyColumnNames.IsSupersetOf);
    }

    private const string Indent = "    ";
}