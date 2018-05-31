using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Lint;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal class LintRenderer : ITemplateRenderer
    {
        public LintRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        protected IDbConnection Connection { get; }

        protected IRelationalDatabase Database { get; }

        protected IHtmlFormatter Formatter { get; }

        protected DirectoryInfo ExportDirectory { get; }

        public void Render()
        {
            var linter = new DatabaseLinter(Connection, Database);
            var messages = linter.AnalyzeDatabase();

            var groupedRules = messages
                .GroupBy(m => m.Title)
                .Select(m => new LintResults.LintRule(m.Key, m.Select(r => r.Message).ToList()))
                .ToList();

            var templateParameter = new LintResults { LintRules = groupedRules };
            var renderedLint = Formatter.RenderTemplate(templateParameter);

            var lintContainer = new Container
            {
                Content = renderedLint,
                DatabaseName = Database.DatabaseName
            };

            var renderedPage = Formatter.RenderTemplate(lintContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "lint.html");

            File.WriteAllText(outputPath, renderedPage);
        }

        public async Task RenderAsync()
        {
            var linter = new DatabaseLinter(Connection, Database);
            var messages = linter.AnalyzeDatabase();

            var groupedRules = messages
                .GroupBy(m => m.Title)
                .Select(m => new LintResults.LintRule(m.Key, m.Select(r => r.Message).ToList()))
                .ToList();

            var templateParameter = new LintResults { LintRules = groupedRules };
            var renderedLint = Formatter.RenderTemplate(templateParameter);

            var lintContainer = new Container
            {
                Content = renderedLint,
                DatabaseName = Database.DatabaseName
            };

            var renderedPage = Formatter.RenderTemplate(lintContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "lint.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
