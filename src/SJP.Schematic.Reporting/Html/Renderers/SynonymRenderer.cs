using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class SynonymRenderer : IDataRenderer
{
    public Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new SynonymModelMapper();
        var dataDirectory = new DirectoryInfo(Path.Combine(context.ExportDirectory.FullName, "data", "synonyms"));

        return RenderTaskRunner.RunAllAsync(
            data.Synonyms,
            static s => $"synonym '{s.Name.ToVisibleName()}'",
            (synonym, ct) => RenderSynonymAsync(synonym, mapper, data.SynonymTargets, context, dataDirectory, ct),
            cancellationToken);
    }

    private static async Task RenderSynonymAsync(
        IDatabaseSynonym synonym,
        SynonymModelMapper mapper,
        SynonymTargets synonymTargets,
        RenderContext context,
        DirectoryInfo dataDirectory,
        CancellationToken cancellationToken)
    {
        var viewModel = mapper.Map(synonym, synonymTargets);

        var safeKey = synonym.Name.ToSafeKey();
        var json = context.JsonWriter.Serialize(viewModel);
        context.Bundle.AddDetail("synonym", safeKey, json);

        var outputFile = new FileInfo(Path.Combine(dataDirectory.FullName, safeKey + ".json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
