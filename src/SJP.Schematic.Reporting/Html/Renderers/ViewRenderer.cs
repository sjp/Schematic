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
    internal class ViewRenderer : ITemplateRenderer
    {
        public ViewRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

            if (exportDirectory == null)
                throw new ArgumentNullException(nameof(exportDirectory));

            ExportDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "views"));
        }

        protected IDbConnection Connection { get; }

        protected IRelationalDatabase Database { get; }

        protected IHtmlFormatter Formatter { get; }

        protected DirectoryInfo ExportDirectory { get; }

        public void Render()
        {
            var views = Database.Views.ToList();
            var mapper = new ViewModelMapper(Connection, Database.Dialect);

            foreach (var view in views)
            {
                var viewModel = mapper.Map(view);
                var renderedView = Formatter.RenderTemplate(viewModel);

                var viewContainer = new Container
                {
                    Content = renderedView,
                    DatabaseName = Database.DatabaseName,
                    RootPath = "../"
                };
                var renderedPage = Formatter.RenderTemplate(viewContainer);

                var outputPath = Path.Combine(ExportDirectory.FullName, view.Name.ToSafeKey() + ".html");
                if (!ExportDirectory.Exists)
                    ExportDirectory.Create();

                File.WriteAllText(outputPath, renderedPage);
            }
        }

        public async Task RenderAsync()
        {
            var views = await Database.ViewsAsync().ConfigureAwait(false);
            var mapper = new ViewModelMapper(Connection, Database.Dialect);

            await views.ForEachAsync(async view =>
            {
                var viewModel = await mapper.MapAsync(view).ConfigureAwait(false);
                var renderedView = Formatter.RenderTemplate(viewModel);

                var viewContainer = new Container
                {
                    Content = renderedView,
                    DatabaseName = Database.DatabaseName,
                    RootPath = "../"
                };
                var renderedPage = Formatter.RenderTemplate(viewContainer);

                var outputPath = Path.Combine(ExportDirectory.FullName, view.Name.ToSafeKey() + ".html");
                if (!ExportDirectory.Exists)
                    ExportDirectory.Create();

                using (var writer = File.CreateText(outputPath))
                    await writer.WriteAsync(renderedPage).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
