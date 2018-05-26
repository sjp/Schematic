using System.Diagnostics;
using System.Reflection;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SchemaSpy.Html.ViewModels
{
    internal class Container : ITemplateParameter
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

        private string _databaseName = "Database";

        public string Content { get; set; }

        public string ProjectVersion => _projectVersion;

        private static readonly string _projectVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

    }
}
