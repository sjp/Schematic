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

internal sealed class ViewsRenderer : ITemplateRenderer
{
    public ViewsRenderer(
        IIdentifierDefaults identifierDefaults,
        IHtmlFormatter formatter,
        IEnumerable<IDatabaseView> views,
        DirectoryInfo exportDirectory)
    {
        Views = views ?? throw new ArgumentNullException(nameof(views));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IIdentifierDefaults IdentifierDefaults { get; }

    private IHtmlFormatter Formatter { get; }

    private IEnumerable<IDatabaseView> Views { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new MainModelMapper();

        var viewViewModels = Views.Select(mapper.Map).ToList();

        var viewsVm = new Views(viewViewModels);
        var renderedMain = await Formatter.RenderTemplateAsync(viewsVm, cancellationToken).ConfigureAwait(false);

        var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
            ? IdentifierDefaults.Database + " Database"
            : "Database";
        var pageTitle = "Views · " + databaseName;
        var mainContainer = new Container(renderedMain, pageTitle, string.Empty);
        var renderedPage = await Formatter.RenderTemplateAsync(mainContainer, cancellationToken).ConfigureAwait(false);

        if (!ExportDirectory.Exists)
            ExportDirectory.Create();
        var outputPath = Path.Combine(ExportDirectory.FullName, "views.html");

        using var writer = File.CreateText(outputPath);
        await writer.WriteAsync(renderedPage.AsMemory(), cancellationToken).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);
    }
}