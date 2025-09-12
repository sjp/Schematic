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

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class ViewRenderer : ITemplateRenderer
{
    public ViewRenderer(
        IIdentifierDefaults identifierDefaults,
        IHtmlFormatter formatter,
        IEnumerable<IDatabaseView> views,
        ReferencedObjectTargets referencedObjectTargets,
        DirectoryInfo exportDirectory
    )
    {
        Views = views ?? throw new ArgumentNullException(nameof(views));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        ReferencedObjectTargets = referencedObjectTargets ?? throw new ArgumentNullException(nameof(referencedObjectTargets));

        ArgumentNullException.ThrowIfNull(exportDirectory);

        ExportDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "views"));
    }

    private IIdentifierDefaults IdentifierDefaults { get; }

    private IHtmlFormatter Formatter { get; }

    private IEnumerable<IDatabaseView> Views { get; }

    private ReferencedObjectTargets ReferencedObjectTargets { get; }

    private DirectoryInfo ExportDirectory { get; }

    public Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new ViewModelMapper();

        var viewTasks = Views.Select(async view =>
        {
            var viewModel = mapper.Map(view, ReferencedObjectTargets);
            var renderedView = await Formatter.RenderTemplateAsync(viewModel, cancellationToken).ConfigureAwait(false);

            var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? IdentifierDefaults.Database + " Database"
                : "Database";
            var pageTitle = view.Name.ToVisibleName() + " · View · " + databaseName;
            var viewContainer = new Container(renderedView, pageTitle, "../");
            var renderedPage = await Formatter.RenderTemplateAsync(viewContainer, cancellationToken).ConfigureAwait(false);

            var outputPath = Path.Combine(ExportDirectory.FullName, view.Name.ToSafeKey() + ".html");
            if (!ExportDirectory.Exists)
                ExportDirectory.Create();

            await using var writer = File.CreateText(outputPath);
            await writer.WriteAsync(renderedPage.AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        });

        return Task.WhenAll(viewTasks);
    }
}