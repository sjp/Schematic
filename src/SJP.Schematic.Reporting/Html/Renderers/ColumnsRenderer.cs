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

internal sealed class ColumnsRenderer : ITemplateRenderer
{
    public ColumnsRenderer(
        IIdentifierDefaults identifierDefaults,
        IHtmlFormatter formatter,
        IEnumerable<IRelationalDatabaseTable> tables,
        IEnumerable<IDatabaseView> views,
        DirectoryInfo exportDirectory)
    {
        if (tables == null)
            throw new ArgumentNullException(nameof(tables));
        if (views == null)
            throw new ArgumentNullException(nameof(views));

        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        Tables = tables;
        Views = views;
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IIdentifierDefaults IdentifierDefaults { get; }

    private IHtmlFormatter Formatter { get; }

    private IEnumerable<IRelationalDatabaseTable> Tables { get; }

    private IEnumerable<IDatabaseView> Views { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new ColumnsModelMapper();

        var tableColumnViewModels = Tables.SelectMany(mapper.Map).Select(static vm => vm as Columns.Column);
        var viewColumnViewModels = Views.SelectMany(mapper.Map).Select(static vm => vm as Columns.Column);

        var orderedColumns = tableColumnViewModels
            .Concat(viewColumnViewModels)
            .OrderBy(static c => c.Name)
            .ThenBy(static c => c.Ordinal)
            .ToList();

        var templateParameter = new Columns(orderedColumns);
        var renderedColumns = await Formatter.RenderTemplateAsync(templateParameter, cancellationToken).ConfigureAwait(false);

        var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
            ? IdentifierDefaults.Database + " Database"
            : "Database";
        var pageTitle = "Columns · " + databaseName;
        var columnsContainer = new Container(renderedColumns, pageTitle, string.Empty);
        var renderedPage = await Formatter.RenderTemplateAsync(columnsContainer, cancellationToken).ConfigureAwait(false);

        if (!ExportDirectory.Exists)
            ExportDirectory.Create();
        var outputPath = Path.Combine(ExportDirectory.FullName, "columns.html");

        using var writer = File.CreateText(outputPath);
        await writer.WriteAsync(renderedPage.AsMemory(), cancellationToken).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);
    }
}