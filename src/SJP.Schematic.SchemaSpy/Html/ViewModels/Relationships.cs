using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SchemaSpy.Html.ViewModels
{
    internal class Relationships : ITemplateParameter
    {
        public SchemaSpyTemplate Template { get; } = SchemaSpyTemplate.Relationships;

        public IEnumerable<Diagram> Diagrams
        {
            get => _diagrams;
            set => _diagrams = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<Diagram> _diagrams = Enumerable.Empty<Diagram>();

        internal class Diagram
        {
            public Diagram(string diagramName, string dotDefinition)
            {
                if (diagramName.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(diagramName));
                Name = diagramName;

                if (dotDefinition.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(dotDefinition));
                Dot = dotDefinition;
            }

            public string Name { get; }

            public string ContainerId => Name.ToLowerInvariant() + "-chart";

            public bool IsActive { get; set; }

            public string ActiveClass => IsActive ? "class=\"active\"" : string.Empty;

            public string ActiveText => IsActive ? "active" : string.Empty;

            public string Dot { get; }
        }
    }
}
