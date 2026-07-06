using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class OrphansRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var orphanedTables = data.Tables
            .Where(static t => t.ParentKeys.Empty() && t.ChildKeys.Empty())
            .ToList();

        var mapper = new OrphansModelMapper();
        var orphanedTableViewModels = orphanedTables.ConvertAll(t =>
        {
            if (!data.RowCounts.TryGetValue(t.Name, out var rowCount))
                rowCount = 0;
            return mapper.Map(t, rowCount);
        });

        var orphansVm = new Orphans(orphanedTableViewModels);

        var json = context.JsonWriter.Serialize(orphansVm);
        context.Bundle.AddSummary("orphans", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "orphans.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
