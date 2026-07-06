using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class SequencesRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new MainModelMapper();

        var sequenceViewModels = data.Sequences.Select(mapper.Map).ToList();
        var sequencesVm = new Sequences(sequenceViewModels);

        var json = context.JsonWriter.Serialize(sequencesVm);
        context.Bundle.AddSummary("sequences", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "sequences.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
