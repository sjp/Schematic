using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class RoutineRenderer : IDataRenderer
{
    public Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new RoutineModelMapper();
        var dataDirectory = new DirectoryInfo(Path.Combine(context.ExportDirectory.FullName, "data", "routines"));

        return RenderTaskRunner.RunAllAsync(
            data.Routines,
            static r => $"routine '{r.Name.ToVisibleName()}'",
            (routine, ct) => RenderRoutineAsync(routine, mapper, context, dataDirectory, ct),
            cancellationToken);
    }

    private static async Task RenderRoutineAsync(
        IDatabaseRoutine routine,
        RoutineModelMapper mapper,
        RenderContext context,
        DirectoryInfo dataDirectory,
        CancellationToken cancellationToken)
    {
        var viewModel = mapper.Map(routine);

        var safeKey = routine.Name.ToSafeKey();
        var json = context.JsonWriter.Serialize(viewModel);
        context.Bundle.AddDetail("routine", safeKey, json);

        var outputFile = new FileInfo(Path.Combine(dataDirectory.FullName, safeKey + ".json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
