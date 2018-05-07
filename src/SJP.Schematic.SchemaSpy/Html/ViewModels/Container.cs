using System;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SchemaSpy.Html.ViewModels
{
    public class Container : ITemplateParameter
    {
        public SchemaSpyTemplate Template { get; } = SchemaSpyTemplate.Container;

        public string RootPath { get; set; }

        public string DatabaseName
        {
            get => _databaseName;
            set
            {
                _databaseName = value.IsNullOrWhiteSpace()
                    ? "Database"
                    : value + " Database";
            }
        }

        public string Content { get; set; }

        // TODO -- change this to be based on the current assembly file version
        public string ProjectVersion
        {
            get
            {
                var version = new Version(1, 0, 0);
                return version.ToString();
            }
        }

        public string PageScript
        {
            get => _pageScript;
            set
            {
                if (value.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(value));

                _pageScript = value;
            }
        }

        private string _databaseName = "Database";
        private string _pageScript = "main.js";
    }
}
