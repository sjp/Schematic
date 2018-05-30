using System.Diagnostics;
using System.Reflection;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    internal class Container : ITemplateParameter
    {
        public ReportTemplate Template { get; } = ReportTemplate.Container;

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

        private readonly static string _projectVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

    }
}
