using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class RoutinesRenderer : IDataRenderer
{
    public RoutinesRenderer(
        IEnumerable<IDatabaseRoutine> routines,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory)
    {
        Routines = routines ?? throw new ArgumentNullException(nameof(routines));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IEnumerable<IDatabaseRoutine> Routines { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new MainModelMapper();

        var routineViewModels = Routines.Select(mapper.Map).ToList();
        var routinesVm = new Routines(routineViewModels);

        var json = JsonWriter.Serialize(routinesVm);
        Bundle.AddSummary("routines", json);

        var outputFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "routines.json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
