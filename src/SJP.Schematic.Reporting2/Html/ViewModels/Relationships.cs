using System;
using System.Collections.Generic;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Internal. Not intended to be used outside of this assembly. Only required for templating.
/// </summary>
public sealed class Relationships : ITemplateParameter
{
    public Relationships(IEnumerable<Diagram> diagrams)
    {
        Diagrams = diagrams ?? throw new ArgumentNullException(nameof(diagrams));
    }

    public ReportTemplate Template { get; } = ReportTemplate.Relationships;

    public IEnumerable<Diagram> Diagrams { get; }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Diagram
    {
        public Diagram(string diagramName, string dotDefinition, bool isActive)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(diagramName);
            ArgumentException.ThrowIfNullOrWhiteSpace(dotDefinition);

            Name = diagramName;
            Dot = dotDefinition;
            ContainerId = Name.ToLowerInvariant().Replace(' ', '-') + "-chart";
            ActiveClass = isActive ? "active" : string.Empty;
            Selected = isActive ? "true" : "false";
        }

        public string Name { get; }

        public string ContainerId { get; }

        public string ActiveClass { get; }

        public string Selected { get; }

        public string Dot { get; }

        // a bit hacky, needed to render image directly instead of via file
        public string Svg { get; set; } = string.Empty;
    }
}