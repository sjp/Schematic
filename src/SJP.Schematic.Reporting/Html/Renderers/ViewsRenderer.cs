using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class ViewsRenderer : ITemplateRenderer
    {
        public ViewsRenderer(
            IDbConnection connection,
            IRelationalDatabase database,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IDatabaseView> views,
            DirectoryInfo exportDirectory)
        {
            if (views == null || views.AnyNull())
                throw new ArgumentNullException(nameof(views));

            Views = views;

            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IDbConnection Connection { get; }

        private IRelationalDatabase Database { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IDatabaseView> Views { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var mapper = new MainModelMapper(Connection, Database);

            var viewViewModels = new List<Main.View>();
            foreach (var view in Views)
            {
                var renderView = await mapper.MapAsync(view, cancellationToken).ConfigureAwait(false);
                viewViewModels.Add(renderView);
            }

            var viewsVm = new Views(viewViewModels);
            var renderedMain = Formatter.RenderTemplate(viewsVm);

            var databaseName = !Database.IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? Database.IdentifierDefaults.Database + " Database"
                : "Database";
            var pageTitle = "Views — " + databaseName;
            var mainContainer = new Container(renderedMain, pageTitle, string.Empty);
            var renderedPage = Formatter.RenderTemplate(mainContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "views.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
