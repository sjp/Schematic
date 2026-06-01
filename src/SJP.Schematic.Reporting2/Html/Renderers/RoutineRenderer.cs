using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class RoutineRenderer : IDataRenderer
{
    public RoutineRenderer(
        IEnumerable<IDatabaseRoutine> routines,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory
    )
    {
        Routines = routines ?? throw new ArgumentNullException(nameof(routines));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));

        ArgumentNullException.ThrowIfNull(exportDirectory);
        DataDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "data", "routines"));
    }

    private IEnumerable<IDatabaseRoutine> Routines { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo DataDirectory { get; }

    public Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new RoutineModelMapper();

        var routineTasks = new List<Task>();
        foreach (var routine in Routines)
            routineTasks.Add(RenderRoutineAsync(routine, mapper, cancellationToken));

        return Task.WhenAll(routineTasks);
    }

    private async Task RenderRoutineAsync(IDatabaseRoutine routine, RoutineModelMapper mapper, CancellationToken cancellationToken)
    {
        var viewModel = mapper.Map(routine);

        var safeKey = routine.Name.ToSafeKey();
        var json = JsonWriter.Serialize(viewModel);
        Bundle.AddDetail("routine", safeKey, json);

        var outputFile = new FileInfo(Path.Combine(DataDirectory.FullName, safeKey + ".json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
