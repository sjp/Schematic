using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class ColumnsRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new ColumnsModelMapper();

        var tableColumns = data.Tables.SelectMany(mapper.Map);
        var viewColumns = data.Views.SelectMany(mapper.Map);

        var orderedColumns = tableColumns
            .Concat(viewColumns)
            .OrderBy(static c => c.Name, StringComparer.Ordinal)
            .ThenBy(static c => c.Ordinal)
            .ToList();

        var columnsVm = new Columns(orderedColumns);

        var json = context.JsonWriter.Serialize(columnsVm);
        context.Bundle.AddSummary("columns", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "columns.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
