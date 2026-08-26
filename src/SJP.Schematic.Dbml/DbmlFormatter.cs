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
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public string RenderTables(IReadOnlyCollection<IRelationalDatabaseTable> tables)
    {
        ArgumentNullException.ThrowIfNull(tables);

        if (!tables.Any())
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

        var anyForeignKeys = tables.Any(t => GetRenderableParentKeys(t, renderedTableNames).Count > 0);
        if (anyForeignKeys)
        {
            builder.AppendLine();
            foreach (var table in tables)
                RenderForeignKeys(builder, table, renderedTableNames);
        }

        return builder.GetStringAndRelease().TrimEnd();
    }

    private static void RenderTable(StringBuilder builder, IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(table);

        var tableName = table.Name.ToDbmlName();
        builder.Append("Table ")
            .Append(tableName)
            .AppendLine(" {");

        if (table.Columns.Count > 0)
        {
            foreach (var column in table.Columns)
                builder.AppendLine(RenderColumnLine(table, column));
        }

        var indexLines = RenderIndexLines(table);
        if (indexLines.Count > 0)
        {
            builder.AppendLine()
                .Append(Indent)
                .AppendLine("Indexes {");

            foreach (var indexLine in indexLines)
                builder.AppendLine(indexLine);

            builder.Append(Indent)
                .AppendLine("}");
        }

        builder.AppendLine("}");
    }

    private static string RenderColumnLine(IRelationalDatabaseTable table, IDatabaseColumn column)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(column);

        var columnName = column.Name.ToDbmlName();

        var options = new List<string> { column.IsNullable ? "null" : "not null" };

        if (column.AutoIncrement.IsSome)
            options.Add("increment");

        if (ColumnIsPrimaryKey(table, column))
            options.Add("primary key");
        else if (ColumnIsUniqueKey(table, column))
            options.Add("unique");

        column.DefaultValue.IfSome(def => options.Add("default: " + def.ToDbmlDefaultValue()));

        var columnOptions = options.Count > 0
            ? " [" + options.Join(", ") + "]"
            : string.Empty;

        var typeName = column.Type.Definition.RemoveEnclosingQuotingCharacters().ToDbmlTypeName();

        return Indent + columnName + " " + typeName + columnOptions;
    }

    private static List<string> RenderIndexLines(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var compositeKeys = new List<IDatabaseKey>();
        table.PrimaryKey.Filter(static pk => pk.Columns.Count > 1).IfSome(compositeKeys.Add);
        var compositePrimaryKeyCount = compositeKeys.Count;
        compositeKeys.AddRange(table.UniqueKeys.Where(static uk => uk.Columns.Count > 1));

        var result = new List<string>(compositeKeys.Count + table.Indexes.Count);

        for (var i = 0; i < compositeKeys.Count; i++)
            result.Add(RenderKeyIndexLine(compositeKeys[i], i < compositePrimaryKeyCount ? "pk" : "unique"));

        foreach (var index in table.Indexes)
        {
            var isBackingIndex = index.IsUnique
                && compositeKeys.Exists(key => IsIndexForKey(index, key));
            if (!isBackingIndex)
                result.Add(RenderIndexLine(table, index));
        }

        return result;
    }

    private static string RenderKeyIndexLine(IDatabaseKey key, string keyOption)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(keyOption);

        var columns = "(" + key.Columns.Select(static c => c.Name.ToDbmlName()).Join(", ") + ")";

        var options = new List<string>();
        key.Name.IfSome(name => options.Add("name: " + name.ToVisibleName().ToDbmlStringLiteral()));
        options.Add(keyOption);

        return Indent + Indent + columns + " "
            + "[" + options.Join(", ") + "]";
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

    private static string RenderIndexLine(IRelationalDatabaseTable table, IDatabaseIndex index)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(index);

        var columns = index.Columns.Count > 1
            ? "(" + index.Columns.Select(RenderIndexColumn).Join(", ") + ")"
            : RenderIndexColumn(index.Columns.Single());

        var options = new List<string> { "name: " + index.Name.ToVisibleName().ToDbmlStringLiteral() };
        if (index.IsUnique)
            options.Add("unique");

        return Indent + Indent + columns + " "
            + "[" + options.Join(", ") + "]";
    }

    private static string RenderIndexColumn(IDatabaseIndexColumn indexColumn)
    {
        ArgumentNullException.ThrowIfNull(indexColumn);

        if (indexColumn.DependentColumns.Count == 1)
        {
            var columnName = indexColumn.DependentColumns[0].Name;
            var expression = indexColumn.Expression.RemoveEnclosingQuotingCharacters();
            if (string.Equals(expression, columnName.LocalName, StringComparison.Ordinal))
                return columnName.ToDbmlName();
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

    private static void RenderForeignKeys(StringBuilder builder, IRelationalDatabaseTable table, IReadOnlySet<Identifier> renderedTableNames)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(renderedTableNames);

        var parentKeys = GetRenderableParentKeys(table, renderedTableNames);
        if (parentKeys.Count == 0)
            return;

        var childTableName = table.Name.ToDbmlName();
        var uniqueColumnSets = GetUniqueColumnSets(table);

        foreach (var relationalKey in parentKeys)
        {
            var isChildKeyUnique = IsChildKeyUnique(uniqueColumnSets, relationalKey.ChildKey);
            var relationalOperator = isChildKeyUnique ? "-" : ">";

            var parentTableName = relationalKey.ParentTable.ToDbmlName();

            var childRef = childTableName + "." + RenderKeyColumns(relationalKey.ChildKey);
            var parentRef = parentTableName + "." + RenderKeyColumns(relationalKey.ParentKey);

            builder.Append("Ref: ")
                .Append(childRef)
                .Append(' ')
                .Append(relationalOperator)
                .Append(' ')
                .AppendLine(parentRef);
        }
    }

    private static string RenderKeyColumns(IDatabaseKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return key.Columns.Count > 1
            ? "(" + key.Columns.Select(static c => c.Name.ToDbmlName()).Join(", ") + ")"
            : key.Columns.Single().Name.ToDbmlName();
    }

    private static bool ColumnIsPrimaryKey(IRelationalDatabaseTable table, IDatabaseColumn column)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(column);

        return table.PrimaryKey
            .Match(
                pk => pk.Columns.Count == 1
                    && string.Equals(pk.Columns.Single().Name.LocalName, column.Name.LocalName, StringComparison.Ordinal),
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
                    && string.Equals(uk.Columns.Single().Name.LocalName, column.Name.LocalName, StringComparison.Ordinal));
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