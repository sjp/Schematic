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

internal sealed class OrphansRenderer : ITemplateRenderer
{
    public OrphansRenderer(
        IIdentifierDefaults identifierDefaults,
        IHtmlFormatter formatter,
        IEnumerable<IRelationalDatabaseTable> tables,
        IReadOnlyDictionary<Identifier, ulong> rowCounts,
        DirectoryInfo exportDirectory
    )
    {
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        RowCounts = rowCounts ?? throw new ArgumentNullException(nameof(rowCounts));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IIdentifierDefaults IdentifierDefaults { get; }

    private IHtmlFormatter Formatter { get; }

    private IEnumerable<IRelationalDatabaseTable> Tables { get; }

    private IReadOnlyDictionary<Identifier, ulong> RowCounts { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var orphanedTables = Tables
            .Where(static t => t.ParentKeys.Empty() && t.ChildKeys.Empty())
            .ToList();

        var mapper = new OrphansModelMapper();
        var orphanedTableViewModels = orphanedTables
            .ConvertAll(t =>
            {
                if (!RowCounts.TryGetValue(t.Name, out var rowCount))
                    rowCount = 0;
                return mapper.Map(t, rowCount);
            })
;

        var templateParameter = new Orphans(orphanedTableViewModels);
        var renderedOrphans = await Formatter.RenderTemplateAsync(templateParameter, cancellationToken);

        var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
            ? IdentifierDefaults.Database + " Database"
            : "Database";
        var pageTitle = "Orphan Tables · " + databaseName;
        var orphansContainer = new Container(renderedOrphans, pageTitle, string.Empty);
        var renderedPage = await Formatter.RenderTemplateAsync(orphansContainer, cancellationToken);

        if (!ExportDirectory.Exists)
            ExportDirectory.Create();
        var outputPath = Path.Combine(ExportDirectory.FullName, "orphans.html");

        await using var writer = File.CreateText(outputPath);
        await writer.WriteAsync(renderedPage.AsMemory(), cancellationToken);
        await writer.FlushAsync(cancellationToken);
    }
}