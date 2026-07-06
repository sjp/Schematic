using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class RelationshipsRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new RelationshipsModelMapper();
        var viewModel = mapper.Map(data.Tables, data.RowCounts);

        // The diagram is now a graph payload laid out and drawn in the browser, so there is no SVG to
        // render here — just serialize the graph alongside the other report data.
        var json = context.JsonWriter.Serialize(viewModel);
        context.Bundle.AddSummary("relationships", json);

        var dataDirectory = new DirectoryInfo(Path.Combine(context.ExportDirectory.FullName, "data"));
        var outputFile = new FileInfo(Path.Combine(dataDirectory.FullName, "relationships.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
