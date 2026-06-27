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
    public static RelationshipGraph Map(
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        IReadOnlyDictionary<Identifier, ulong> rowCounts,
        Identifier? highlightedTable = null)
    {
        ArgumentNullException.ThrowIfNull(tables);
        ArgumentNullException.ThrowIfNull(rowCounts);

        var tableNames = new HashSet<Identifier>(tables.Select(static t => t.Name));

        var nodes = new List<GraphTable>();
        var edges = new List<GraphEdge>();

        foreach (var table in tables)
        {
            var primaryKey = table.PrimaryKey;
            var uniqueKeys = table.UniqueKeys;
            var parentKeys = table.ParentKeys;

            var columns = table.Columns.Select(col =>
            {
                var columnName = col.Name.LocalName;
                var isPrimaryKey = primaryKey.Match(pk => pk.Columns.Any(c => string.Equals(c.Name.LocalName, columnName, StringComparison.Ordinal)), static () => false);
                var isUniqueKey = uniqueKeys.Any(uk => uk.Columns.Any(c => string.Equals(c.Name.LocalName, columnName, StringComparison.Ordinal)));
                var isForeignKey = parentKeys.Any(fk => fk.ChildKey.Columns.Any(c => string.Equals(c.Name.LocalName, columnName, StringComparison.Ordinal)));

                return new GraphColumn(columnName, col.Type.Definition, col.IsNullable, isPrimaryKey, isUniqueKey, isForeignKey);
            }).ToList();

            if (!rowCounts.TryGetValue(table.Name, out var rowCount))
                rowCount = 0;

            var isHighlighted = highlightedTable != null && table.Name == highlightedTable;

            nodes.Add(new GraphTable(
                table.Name,
                columns,
                parentKeys.UCount(),
                table.ChildKeys.UCount(),
                rowCount,
                isHighlighted
            ));

            foreach (var relationalKey in parentKeys)
            {
                // Only draw an edge when both endpoints are in this graph (e.g. a per-table
                // neighbourhood can reference tables outside its own degree window).
                if (!tableNames.Contains(relationalKey.ParentTable))
                    continue;

                var constraintName = relationalKey.ChildKey.Name.Match(static name => name.LocalName, static () => string.Empty);
                var childColumns = relationalKey.ChildKey.Columns.Select(static c => c.Name.LocalName).ToList();
                var parentColumns = relationalKey.ParentKey.Columns.Select(static c => c.Name.LocalName).ToList();

                edges.Add(new GraphEdge(
                    relationalKey.ChildTable.ToSafeKey(),
                    relationalKey.ParentTable.ToSafeKey(),
                    constraintName,
                    childColumns,
                    parentColumns
                ));
            }
        }

        return new RelationshipGraph(nodes, edges);
    }
}
