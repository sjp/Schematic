﻿using System;
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
    internal sealed class ViewRenderer : ITemplateRenderer
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

        private IDbConnection Connection { get; }

        private IRelationalDatabase Database { get; }

        private IHtmlFormatter Formatter { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var views = await Database.GetAllViews(cancellationToken).ConfigureAwait(false);
            var mapper = new ViewModelMapper(Connection, Database.Dialect);

            var viewTasks = views.Select(async view =>
            {
                var viewModel = await mapper.MapAsync(view, cancellationToken).ConfigureAwait(false);
                var renderedView = Formatter.RenderTemplate(viewModel);

                var viewContainer = new Container(renderedView, Database.IdentifierDefaults.Database, "../");
                var renderedPage = Formatter.RenderTemplate(viewContainer);

                var outputPath = Path.Combine(ExportDirectory.FullName, view.Name.ToSafeKey() + ".html");
                if (!ExportDirectory.Exists)
                    ExportDirectory.Create();

                using (var writer = File.CreateText(outputPath))
                    await writer.WriteAsync(renderedPage).ConfigureAwait(false);
            });
            await Task.WhenAll(viewTasks).ConfigureAwait(false);
        }
    }
}
