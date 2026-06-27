using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// A relationship diagram as plain data — a set of table nodes and the foreign-key edges between
/// them. Replaces the previously pre-rendered Graphviz SVG: the React report lays this out and
/// draws it client-side, so no <c>dot</c> binary is required on the generating machine.
/// </summary>
public sealed class RelationshipGraph
{
    public RelationshipGraph(IEnumerable<GraphTable> nodes, IEnumerable<GraphEdge> edges)
    {
        Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
        Edges = edges ?? throw new ArgumentNullException(nameof(edges));
        NodesCount = nodes.UCount();
        EdgesCount = edges.UCount();
    }

    public IEnumerable<GraphTable> Nodes { get; }

    public uint NodesCount { get; }

    public IEnumerable<GraphEdge> Edges { get; }

    public uint EdgesCount { get; }
}

/// <summary>
/// A table node in a <see cref="RelationshipGraph"/>. <see cref="Id"/> is the table's safe key (the
/// SPA route param), so the diagram can link a node to its detail page exactly as the rest of the UI
/// does.
/// </summary>
public sealed class GraphTable
{
    public GraphTable(
        Identifier name,
        IEnumerable<GraphColumn> columns,
        uint parentKeysCount,
        uint childKeysCount,
        ulong rowCount,
        bool isHighlighted
    )
    {
        ArgumentNullException.ThrowIfNull(name);

        Id = name.ToSafeKey();
        Name = name.ToVisibleName();
        TableUrl = UrlRouter.GetTableUrl(name);

        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        ColumnsCount = columns.UCount();

        ParentKeysCount = parentKeysCount;
        ChildKeysCount = childKeysCount;
        RowCount = rowCount;
        IsHighlighted = isHighlighted;
    }

    /// <summary>The table's safe key — the SPA route param and the node's stable identity.</summary>
    public string Id { get; }

    public string Name { get; }

    /// <summary>In-app hash route to the table's detail page (e.g. <c>#/tables/&lt;safeKey&gt;</c>).</summary>
    public string TableUrl { get; }

    public IEnumerable<GraphColumn> Columns { get; }

    public uint ColumnsCount { get; }

    public uint ParentKeysCount { get; }

    public uint ChildKeysCount { get; }

    public ulong RowCount { get; }

    /// <summary>The focal table of a per-table diagram; drawn with the highlight palette.</summary>
    public bool IsHighlighted { get; }
}

/// <summary>
/// A column row shown inside a <see cref="GraphTable"/> node. Every column carries its key flags so
/// the UI can offer a "compact" view (key columns only) without a second payload.
/// </summary>
public sealed class GraphColumn
{
    public GraphColumn(string name, string type, bool isNullable, bool isPrimaryKey, bool isUniqueKey, bool isForeignKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        Name = name;
        Type = type ?? string.Empty;
        IsNullable = isNullable;
        IsPrimaryKey = isPrimaryKey;
        IsUniqueKey = isUniqueKey;
        IsForeignKey = isForeignKey;
    }

    public string Name { get; }

    public string Type { get; }

    public bool IsNullable { get; }

    public bool IsPrimaryKey { get; }

    public bool IsUniqueKey { get; }

    public bool IsForeignKey { get; }

    /// <summary>True when the column participates in any key — the "compact" view filter.</summary>
    public bool IsKey => IsPrimaryKey || IsUniqueKey || IsForeignKey;
}

/// <summary>
/// A directed foreign-key edge between two <see cref="GraphTable"/> nodes, pointing from the child
/// (referencing) table to the parent (referenced) table.
/// </summary>
public sealed class GraphEdge
{
    public GraphEdge(
        string childTableId,
        string parentTableId,
        string constraintName,
        IEnumerable<string> childColumns,
        IEnumerable<string> parentColumns
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(childTableId);
        ArgumentException.ThrowIfNullOrEmpty(parentTableId);

        ChildTableId = childTableId;
        ParentTableId = parentTableId;
        ConstraintName = constraintName ?? string.Empty;
        ChildColumns = childColumns?.ToList() ?? throw new ArgumentNullException(nameof(childColumns));
        ParentColumns = parentColumns?.ToList() ?? throw new ArgumentNullException(nameof(parentColumns));

        // A pair of tables can be joined by more than one foreign key, so the columns are part of the
        // identity — this keeps React keys stable and unique across multi-FK relationships.
        Id = ChildTableId + ":" + string.Join(",", ChildColumns) + "->" + ParentTableId;
    }

    /// <summary>Stable identity for the edge (unique even with multiple FKs between the same pair).</summary>
    public string Id { get; }

    public string ChildTableId { get; }

    public string ParentTableId { get; }

    public string ConstraintName { get; }

    public IEnumerable<string> ChildColumns { get; }

    public IEnumerable<string> ParentColumns { get; }
}
