using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal class TableRenderer : ITemplateRenderer
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

        protected IDbConnection Connection { get; }

        protected IRelationalDatabase Database { get; }

        protected IHtmlFormatter Formatter { get; }

        protected DirectoryInfo ExportDirectory { get; }

        public void Render()
        {
            var tables = Database.Tables.ToList();
            var mapper = new TableModelMapper(Connection, Database.Dialect);

            foreach (var table in tables)
            {
                var tableModel = mapper.Map(table);
                var renderedTable = Formatter.RenderTemplate(tableModel);

                var tableContainer = new Container(renderedTable, Database.DatabaseName, "../");
                var renderedPage = Formatter.RenderTemplate(tableContainer);

                var outputPath = Path.Combine(ExportDirectory.FullName, table.Name.ToSafeKey() + ".html");
                if (!ExportDirectory.Exists)
                    ExportDirectory.Create();

                File.WriteAllText(outputPath, renderedPage);
            }
        }

        public async Task RenderAsync()
        {
            var tables = await Database.TablesAsync().ConfigureAwait(false);
            var mapper = new TableModelMapper(Connection, Database.Dialect);

            await tables.ForEachAsync(async table =>
            {
                var tableModel = await mapper.MapAsync(table).ConfigureAwait(false);
                var renderedTable = Formatter.RenderTemplate(tableModel);

                var tableContainer = new Container(renderedTable, Database.DatabaseName, "../");
                var renderedPage = Formatter.RenderTemplate(tableContainer);

                var outputPath = Path.Combine(ExportDirectory.FullName, table.Name.ToSafeKey() + ".html");
                if (!ExportDirectory.Exists)
                    ExportDirectory.Create();

                using (var writer = File.CreateText(outputPath))
                    await writer.WriteAsync(renderedPage).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
