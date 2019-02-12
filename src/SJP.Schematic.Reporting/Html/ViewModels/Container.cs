using System.Diagnostics;
using System.Reflection;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Container : ITemplateParameter
    {
        public Container(
            string content,
            string databaseName,
            string rootPath
        )
        {
            Content = content ?? string.Empty;
            DatabaseName = !databaseName.IsNullOrWhiteSpace()
                ? databaseName + " Database"
                : "Database";
            RootPath = rootPath ?? string.Empty;
        }

        public ReportTemplate Template { get; } = ReportTemplate.Container;

        public string RootPath { get; }

        public string DatabaseName { get; }

        public HtmlString Content { get; }

        public string ProjectVersion => _projectVersion;

        private static readonly string _projectVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
    }
}
