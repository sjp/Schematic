using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class TableRenderer : ITemplateRenderer
    {
        public TableRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

            if (exportDirectory == null)
                throw new ArgumentNullException(nameof(exportDirectory));

            ExportDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "tables"));
        }

        private IDbConnection Connection { get; }

        private IRelationalDatabase Database { get; }

        private IHtmlFormatter Formatter { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tables = await Database.TablesAsync(cancellationToken).ConfigureAwait(false);
            var mapper = new TableModelMapper(Connection, Database, Database.Dialect);

            var tableTasks = tables.Select(async table =>
            {
                var tableModel = await mapper.MapAsync(table, cancellationToken).ConfigureAwait(false);
                var renderedTable = Formatter.RenderTemplate(tableModel);

                var tableContainer = new Container(renderedTable, Database.DatabaseName, "../");
                var renderedPage = Formatter.RenderTemplate(tableContainer);

                var outputPath = Path.Combine(ExportDirectory.FullName, table.Name.ToSafeKey() + ".html");
                if (!ExportDirectory.Exists)
                    ExportDirectory.Create();

                using (var writer = File.CreateText(outputPath))
                    await writer.WriteAsync(renderedPage).ConfigureAwait(false);
            });
            await Task.WhenAll(tableTasks).ConfigureAwait(false);
        }
    }
}
