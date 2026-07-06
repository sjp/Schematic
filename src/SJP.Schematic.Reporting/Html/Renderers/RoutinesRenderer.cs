using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class RoutinesRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new MainModelMapper();

        var routineViewModels = data.Routines.Select(mapper.Map).ToList();
        var routinesVm = new Routines(routineViewModels);

        var json = context.JsonWriter.Serialize(routinesVm);
        context.Bundle.AddSummary("routines", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "routines.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
