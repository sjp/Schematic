using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class SequenceRenderer : IDataRenderer
{
    public SequenceRenderer(
        IEnumerable<IDatabaseSequence> sequences,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory
    )
    {
        Sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));

        ArgumentNullException.ThrowIfNull(exportDirectory);
        DataDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "data", "sequences"));
    }

    private IEnumerable<IDatabaseSequence> Sequences { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo DataDirectory { get; }

    public Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new SequenceModelMapper();

        return RenderTaskRunner.RunAllAsync(
            Sequences,
            static s => $"sequence '{s.Name.ToVisibleName()}'",
            (sequence, ct) => RenderSequenceAsync(sequence, mapper, ct),
            cancellationToken);
    }

    private async Task RenderSequenceAsync(IDatabaseSequence sequence, SequenceModelMapper mapper, CancellationToken cancellationToken)
    {
        var viewModel = mapper.Map(sequence);

        var safeKey = sequence.Name.ToSafeKey();
        var json = JsonWriter.Serialize(viewModel);
        Bundle.AddDetail("sequence", safeKey, json);

        var outputFile = new FileInfo(Path.Combine(DataDirectory.FullName, safeKey + ".json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
