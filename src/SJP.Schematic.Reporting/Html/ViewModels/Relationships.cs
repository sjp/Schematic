using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    public sealed class Relationships : ITemplateParameter
    {
        public Relationships(IEnumerable<Diagram> diagrams)
        {
            Diagrams = diagrams ?? throw new ArgumentNullException(nameof(diagrams));
        }

        public ReportTemplate Template { get; } = ReportTemplate.Relationships;

        public IEnumerable<Diagram> Diagrams { get; }

        public sealed class Diagram
        {
            public Diagram(string diagramName, string dotDefinition, bool isActive)
            {
                if (diagramName.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(diagramName));
                Name = diagramName;

                if (dotDefinition.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(dotDefinition));
                Dot = dotDefinition;

                ContainerId = Name.ToLowerInvariant() + "-chart";
                ActiveClass = isActive ? "class=\"active\"" : string.Empty;
                ActiveText = isActive ? "active" : string.Empty;
            }

            public string Name { get; }

            public HtmlString ContainerId { get; }

            public HtmlString ActiveClass { get; }

            public HtmlString ActiveText { get; }

            public HtmlString Dot { get; }
        }
    }
}
