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
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <c>null</c>.</exception>
    public string RenderTables(IEnumerable<IRelationalDatabaseTable> tables)
    {
        if (tables == null)
            throw new ArgumentNullException(nameof(tables));

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

        var anyForeignKeys = tables.Any(t => t.ParentKeys.Count > 0);
        if (anyForeignKeys)
        {
            builder.AppendLine();
            foreach (var table in tables)
                RenderForeignKeys(builder, table);
        }

        return builder.GetStringAndRelease().TrimEnd();
    }

    private void RenderTable(StringBuilder builder, IRelationalDatabaseTable table)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        if (table == null)
            throw new ArgumentNullException(nameof(table));

        var tableName = table.Name.ToVisibleName();
        builder.Append("Table ")
            .Append(tableName)
            .AppendLine(" {");

        if (table.Columns.Count > 0)
        {
            foreach (var column in table.Columns)
                builder.AppendLine(RenderColumnLine(table, column));
        }

        if (table.Indexes.Count > 0)
        {
            builder.AppendLine()
                .Append(Indent)
                .AppendLine("Indexes {");

            foreach (var index in table.Indexes)
                builder.AppendLine(RenderIndexLine(table, index));

            builder.Append(Indent)
                .AppendLine("}");
        }

        builder.AppendLine("}");
    }

    private static string RenderColumnLine(IRelationalDatabaseTable table, IDatabaseColumn column)
    {
        if (table == null)
            throw new ArgumentNullException(nameof(table));
        if (column == null)
            throw new ArgumentNullException(nameof(column));

        var columnName = column.Name.ToVisibleName();

        var options = new List<string> { column.IsNullable ? "null" : "not null" };

        if (column.AutoIncrement.IsSome)
            options.Add("increment");

        if (ColumnIsPrimaryKey(table, column))
            options.Add("primary key");
        else if (ColumnIsUniqueKey(table, column))
            options.Add("unique key");

        column.DefaultValue.IfSome(def => options.Add("default: \"" + def.Replace("\"", "\\\"", StringComparison.Ordinal) + "\""));

        var columnOptions = options.Count > 0
            ? " [" + options.Join(", ") + "]"
            : string.Empty;

        return Indent + columnName + " " + column.Type.Definition.RemoveQuotingCharacters() + columnOptions;
    }

    private static string RenderIndexLine(IRelationalDatabaseTable table, IDatabaseIndex index)
    {
        if (table == null)
            throw new ArgumentNullException(nameof(table));
        if (index == null)
            throw new ArgumentNullException(nameof(index));

        var columns = index.Columns.Count > 1
            ? "(" + index.Columns.Select(static ic => ic.Expression).Join(", ").RemoveQuotingCharacters() + ")"
            : index.Columns.Single().Expression.RemoveQuotingCharacters();

        var options = new List<string> { "name: '" + index.Name.ToVisibleName() + "'" };
        if (index.IsUnique)
            options.Add("unique");

        return Indent + Indent + columns + " "
            + "[" + options.Join(", ") + "]";
    }

    private static void RenderForeignKeys(StringBuilder builder, IRelationalDatabaseTable table)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        if (table == null)
            throw new ArgumentNullException(nameof(table));

        if (table.ParentKeys.Count == 0)
            return;

        var childTableName = table.Name.ToVisibleName();

        foreach (var relationalKey in table.ParentKeys)
        {
            var lines = new List<string>();

            var isChildKeyUnique = IsChildKeyUnique(table, relationalKey.ChildKey);
            var relationalOperator = isChildKeyUnique ? "-" : ">";

            var parentTableName = relationalKey.ParentTable.ToVisibleName();

            var columnPairs = relationalKey.ChildKey.Columns.Zip(relationalKey.ParentKey.Columns);
            foreach (var columnPair in columnPairs)
            {
                var childColumn = columnPair.Item1.Name.ToVisibleName();
                var parentColumn = columnPair.Item2.Name.ToVisibleName();

                var childRef = childTableName + "." + childColumn;
                var parentRef = parentTableName + "." + parentColumn;

                var result = "Ref: " + childRef + " " + relationalOperator + " " + parentRef;
                lines.Add(result);
            }

            foreach (var line in lines)
                builder.AppendLine(line);
        }
    }

    private static bool ColumnIsPrimaryKey(IRelationalDatabaseTable table, IDatabaseColumn column)
    {
        if (table == null)
            throw new ArgumentNullException(nameof(table));
        if (column == null)
            throw new ArgumentNullException(nameof(column));

        return table.PrimaryKey
            .Match(
                pk => pk.Columns.Count == 1
                    && string.Equals(pk.Columns.Single().Name.LocalName, column.Name.LocalName, StringComparison.Ordinal),
                static () => false
            );
    }

    private static bool ColumnIsUniqueKey(IRelationalDatabaseTable table, IDatabaseColumn column)
    {
        if (table == null)
            throw new ArgumentNullException(nameof(table));
        if (column == null)
            throw new ArgumentNullException(nameof(column));

        return table.UniqueKeys
            .Any(
                uk => uk.Columns.Count == 1
                    && string.Equals(uk.Columns.Single().Name.LocalName, column.Name.LocalName, StringComparison.Ordinal));
    }

    private static bool IsChildKeyUnique(IRelationalDatabaseTable table, IDatabaseKey key)
    {
        if (table == null)
            throw new ArgumentNullException(nameof(table));
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        var keyColumnNames = key.Columns.Select(static c => c.Name.LocalName).ToList();
        var matchesPkColumns = table.PrimaryKey.Match(pk =>
        {
            var pkColumnNames = pk.Columns.Select(static c => c.Name.LocalName).ToList();
            return keyColumnNames.SequenceEqual(pkColumnNames, StringComparer.Ordinal);
        }, static () => false);
        if (matchesPkColumns)
            return true;

        var matchesUkColumns = table.UniqueKeys.Any(uk =>
        {
            var ukColumnNames = uk.Columns.Select(static c => c.Name.LocalName).ToList();
            return keyColumnNames.SequenceEqual(ukColumnNames, StringComparer.Ordinal);
        });
        if (matchesUkColumns)
            return true;

        var uniqueIndexes = table.Indexes.Where(static i => i.IsUnique).ToList();
        if (uniqueIndexes.Count == 0)
            return false;

        return uniqueIndexes.Any(i =>
        {
            var indexColumnExpressions = i.Columns
                .Select(static ic => ic.DependentColumns.Select(dc => dc.Name.LocalName).FirstOrDefault() ?? ic.Expression)
                .ToList();
            return keyColumnNames.SequenceEqual(indexColumnExpressions, StringComparer.Ordinal);
        });
    }

    private const string Indent = "    ";
}