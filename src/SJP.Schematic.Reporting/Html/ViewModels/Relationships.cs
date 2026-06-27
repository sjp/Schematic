using System;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The schema-wide relationships payload (<c>data/relationships.json</c>): a single
/// <see cref="RelationshipGraph"/> of every table and its foreign keys. The report lays this out and
/// renders it in the browser; the "Compact" (key columns only) and "Large" (all columns) views are a
/// client-side toggle over the same data rather than two pre-rendered diagrams.
/// </summary>
public sealed class Relationships
{
    public Relationships(RelationshipGraph graph)
    {
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));
    }

    public RelationshipGraph Graph { get; }
}
