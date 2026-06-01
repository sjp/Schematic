using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The schema-wide relationships payload (<c>data/relationships.json</c>): one entry per diagram
/// "level" (Compact / Large), each referencing a pre-rendered SVG under <c>data/diagrams/</c>.
/// </summary>
public sealed class Relationships : ITemplateParameter
{
    public Relationships(IEnumerable<RelationshipDiagram> diagrams)
    {
        Diagrams = diagrams ?? throw new ArgumentNullException(nameof(diagrams));
        DiagramsCount = diagrams.UCount();
    }

    [JsonIgnore]
    public ReportTemplate Template { get; } = ReportTemplate.Relationships;

    public IEnumerable<RelationshipDiagram> Diagrams { get; }

    public uint DiagramsCount { get; }

    /// <summary>
    /// A schema-level relationship diagram. Named distinctly from <see cref="Table.Diagram"/> so
    /// the JSON source generator emits non-colliding metadata (both have the simple name "Diagram"
    /// otherwise).
    /// </summary>
    public sealed class RelationshipDiagram
    {
        public RelationshipDiagram(string diagramName, string dotDefinition, bool isActive)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(diagramName);
            ArgumentException.ThrowIfNullOrWhiteSpace(dotDefinition);

            Name = diagramName;
            Dot = dotDefinition;
            ContainerId = "relationships-" + Name.ToLowerInvariant().Replace(' ', '-') + "-chart";
            IsActive = isActive;
            SvgFile = "data/diagrams/" + ContainerId + ".svg";
        }

        public string Name { get; }

        public string ContainerId { get; }

        public bool IsActive { get; }

        public string SvgFile { get; }

        // The DOT source is consumed by the renderer to produce the SVG file; it is not part of
        // the JSON payload.
        [JsonIgnore]
        public string Dot { get; }
    }
}
