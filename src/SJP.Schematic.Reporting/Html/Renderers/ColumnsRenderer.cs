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
    internal sealed class ColumnsRenderer : ITemplateRenderer
    {
        public ColumnsRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
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

        public void Render()
        {
            var tables = Database.Tables.ToList();
            var views = Database.Views.ToList();

            var mapper = new ColumnsModelMapper(Connection, Database.Dialect);

            var tableColumnViewModels = tables.SelectMany(mapper.Map).Select(vm => vm as Columns.Column);
            var viewColumnViewModels = views.SelectMany(mapper.Map).Select(vm => vm as Columns.Column);

            var orderedColumns = tableColumnViewModels.Concat(viewColumnViewModels)
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Ordinal)
                .ToList();

            var templateParameter = new Columns(orderedColumns);
            var renderedColumns = Formatter.RenderTemplate(templateParameter);

            var columnsContainer = new Container(renderedColumns, Database.DatabaseName, string.Empty);
            var renderedPage = Formatter.RenderTemplate(columnsContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "columns.html");

            File.WriteAllText(outputPath, renderedPage);
        }

        public async Task RenderAsync()
        {
            var tables = await Database.TablesAsync().ConfigureAwait(false);
            var views = await Database.ViewsAsync().ConfigureAwait(false);

            var mapper = new ColumnsModelMapper(Connection, Database.Dialect);

            var tableColumnViewModels = tables.SelectMany(mapper.Map).Select(vm => vm as Columns.Column);
            var viewColumnViewModels = views.SelectMany(mapper.Map).Select(vm => vm as Columns.Column);

            var orderedColumns = tableColumnViewModels.Concat(viewColumnViewModels)
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Ordinal)
                .ToList();

            var templateParameter = new Columns(orderedColumns);
            var renderedColumns = Formatter.RenderTemplate(templateParameter);

            var columnsContainer = new Container(renderedColumns, Database.DatabaseName, string.Empty);
            var renderedPage = Formatter.RenderTemplate(columnsContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "columns.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
