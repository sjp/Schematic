using System;
using System.Collections.Generic;
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
    internal sealed class TriggerRenderer : ITemplateRenderer
    {
        public TriggerRenderer(
            IHtmlFormatter formatter,
            IEnumerable<IRelationalDatabaseTable> tables,
            DirectoryInfo exportDirectory
        )
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            Tables = tables;
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

            if (exportDirectory == null)
                throw new ArgumentNullException(nameof(exportDirectory));

            ExportDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "tables"));
        }

        private IHtmlFormatter Formatter { get; }

        private IEnumerable<IRelationalDatabaseTable> Tables { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            var mapper = new TriggerModelMapper();

            var triggerTasks = Tables.SelectMany(table =>
            {
                var outputDirectory = Path.Combine(
                    ExportDirectory.FullName,
                    table.Name.ToSafeKey(),
                    "triggers"
                );

                return table.Triggers.Select(async trigger =>
                {
                    if (!Directory.Exists(outputDirectory))
                        _ = Directory.CreateDirectory(outputDirectory);

                    var triggerModel = mapper.Map(table.Name, trigger);
                    var outputPath = Path.Combine(outputDirectory, trigger.Name.ToSafeKey() + ".html");

                    var renderedTable = await Formatter.RenderTemplateAsync(triggerModel, cancellationToken).ConfigureAwait(false);

                    var pageTitle = trigger.Name.ToVisibleName() + " · Trigger · " + table.Name.ToVisibleName();
                    var container = new Container(renderedTable, pageTitle, "../../../");
                    var renderedPage = await Formatter.RenderTemplateAsync(container, cancellationToken).ConfigureAwait(false);

                    using var writer = File.CreateText(outputPath);
                    await writer.WriteAsync(renderedPage.AsMemory(), cancellationToken).ConfigureAwait(false);
                    await writer.FlushAsync().ConfigureAwait(false);
                });
            });

            await Task.WhenAll(triggerTasks).ConfigureAwait(false);
        }
    }
}
