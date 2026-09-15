using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class SequenceRenderer : IDataRenderer
{
    public Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new SequenceModelMapper();
        var dataDirectory = new DirectoryInfo(Path.Combine(context.ExportDirectory.FullName, "data", "sequences"));

        return RenderTaskRunner.RunAllAsync(
            data.Sequences,
            static s => $"sequence '{s.Name.ToVisibleName()}'",
            (sequence, ct) => RenderSequenceAsync(sequence, mapper, data.UserDefinedTypeTargets, context, dataDirectory, ct),
            cancellationToken);
    }

    private static async Task RenderSequenceAsync(
        IDatabaseSequence sequence,
        SequenceModelMapper mapper,
        UserDefinedTypeTargets userDefinedTypeTargets,
        RenderContext context,
        DirectoryInfo dataDirectory,
        CancellationToken cancellationToken)
    {
        var viewModel = mapper.Map(sequence, userDefinedTypeTargets);

        var safeKey = sequence.Name.ToSafeKey();
        var outputFile = new FileInfo(Path.Combine(dataDirectory.FullName, safeKey + ".json"));
        await context.JsonWriter.SerializeToFileAsync(outputFile, viewModel, cancellationToken);
        context.Bundle.AddDetail("sequence", safeKey, outputFile);
    }
}
