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

internal sealed class SequencesRenderer : IDataRenderer
{
    public SequencesRenderer(
        IEnumerable<IDatabaseSequence> sequences,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory)
    {
        Sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IEnumerable<IDatabaseSequence> Sequences { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new MainModelMapper();

        var sequenceViewModels = Sequences.Select(mapper.Map).ToList();
        var sequencesVm = new Sequences(sequenceViewModels);

        var json = JsonWriter.Serialize(sequencesVm);
        Bundle.AddSummary("sequences", json);

        var outputFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "sequences.json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
