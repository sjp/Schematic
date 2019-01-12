using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Lint;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class LintRenderer : ITemplateRenderer
    {
        public LintRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IDbConnection Connection { get; }

        private IRelationalDatabase Database { get; }

        private IHtmlFormatter Formatter { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var linter = new DatabaseLinter(Connection, Database);
            var messages = await linter.AnalyseDatabaseAsync(cancellationToken).ConfigureAwait(false);

            var groupedRules = messages
                .GroupBy(m => m.Title)
                .Select(m => new LintResults.LintRule(m.Key, m.Select(r => new HtmlString(r.Message)).ToList()))
                .ToList();

            var templateParameter = new LintResults(groupedRules);
            var renderedLint = Formatter.RenderTemplate(templateParameter);

            var lintContainer = new Container(renderedLint, Database.IdentifierDefaults.Database, string.Empty);
            var renderedPage = Formatter.RenderTemplate(lintContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "lint.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
