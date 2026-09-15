using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

/// <summary>
/// Builds a <see cref="RelationshipGraph"/> (table nodes + foreign-key edges) from a set of tables.
/// Replaces the Graphviz/DOT generation that previously produced these diagrams; the column key-flag
/// derivation and the "only connect tables present in the set" edge rule mirror that old behaviour so
/// the diagrams stay equivalent.
/// </summary>
internal static class RelationshipGraphMapper
{
    public static RelationshipGraph Map(IReadOnlyCollection<IRelationalDatabaseTable> tables)
    {
        ArgumentNullException.ThrowIfNull(tables);

        // Safe keys are expensive to compute (slugging plus several SHA-512 hashes), so each table's
        // key is computed once and shared by its node and every edge that touches it. The map doubles
        // as the set of tables in this graph, which decides which edges are drawn.
        var tableIds = new Dictionary<Identifier, string>(tables.Count);
        foreach (var table in tables)
            tableIds.TryAdd(table.Name, table.Name.ToSafeKey());

        var nodes = new List<GraphTable>(tables.Count);
        var edges = new List<GraphEdge>();

        var primaryKeyColumns = new HashSet<string>(StringComparer.Ordinal);
        var uniqueKeyColumns = new HashSet<string>(StringComparer.Ordinal);
        var foreignKeyColumns = new HashSet<string>(StringComparer.Ordinal);

        foreach (var table in tables)
        {
            var parentKeys = table.ParentKeys;

            primaryKeyColumns.Clear();
            uniqueKeyColumns.Clear();
            foreignKeyColumns.Clear();

            table.PrimaryKey.IfSome(primaryKey => AddColumnNames(primaryKeyColumns, primaryKey));
            foreach (var uniqueKey in table.UniqueKeys)
                AddColumnNames(uniqueKeyColumns, uniqueKey);
            foreach (var parentKey in parentKeys)
                AddColumnNames(foreignKeyColumns, parentKey.ChildKey);

            var columns = new List<GraphColumn>(table.Columns.Count);
            foreach (var col in table.Columns)
            {
                var columnName = col.Name.LocalName;
                columns.Add(new GraphColumn(
                    columnName,
                    col.Type.Definition,
                    col.IsNullable,
                    primaryKeyColumns.Contains(columnName),
                    uniqueKeyColumns.Contains(columnName),
                    foreignKeyColumns.Contains(columnName)
                ));
            }

            var tableId = tableIds[table.Name];
            nodes.Add(new GraphTable(
                table.Name,
                tableId,
                columns,
                parentKeys.UCount(),
                table.ChildKeys.UCount()
            ));

            foreach (var relationalKey in parentKeys)
            {
                // Only draw an edge when both endpoints are in this graph, e.g. when the report covers
                // a subset of the database's tables.
                if (!tableIds.TryGetValue(relationalKey.ParentTable, out var parentTableId))
                    continue;

                var childTableId = relationalKey.ChildTable == table.Name
                    ? tableId
                    : relationalKey.ChildTable.ToSafeKey();

                var constraintName = relationalKey.ChildKey.Name.Match(static name => name.LocalName, static () => string.Empty);
                var childColumns = relationalKey.ChildKey.Columns.Select(static c => c.Name.LocalName).ToList();
                var parentColumns = relationalKey.ParentKey.Columns.Select(static c => c.Name.LocalName).ToList();

                edges.Add(new GraphEdge(
                    childTableId,
                    parentTableId,
                    constraintName,
                    childColumns,
                    parentColumns
                ));
            }
        }

        return new RelationshipGraph(nodes, edges);
    }

    private static void AddColumnNames(HashSet<string> columnNames, IDatabaseKey key)
    {
        foreach (var column in key.Columns)
            columnNames.Add(column.Name.LocalName);
    }
}
