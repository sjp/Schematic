using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class SynonymRenderer : IDataRenderer
{
    public SynonymRenderer(
        IEnumerable<IDatabaseSynonym> synonyms,
        SynonymTargets synonymTargets,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory
    )
    {
        Synonyms = synonyms ?? throw new ArgumentNullException(nameof(synonyms));
        SynonymTargets = synonymTargets ?? throw new ArgumentNullException(nameof(synonymTargets));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));

        ArgumentNullException.ThrowIfNull(exportDirectory);
        DataDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "data", "synonyms"));
    }

    private IEnumerable<IDatabaseSynonym> Synonyms { get; }

    private SynonymTargets SynonymTargets { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo DataDirectory { get; }

    public Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new SynonymModelMapper();

        var synonymTasks = new List<Task>();
        foreach (var synonym in Synonyms)
            synonymTasks.Add(RenderSynonymAsync(synonym, mapper, cancellationToken));

        return Task.WhenAll(synonymTasks);
    }

    private async Task RenderSynonymAsync(IDatabaseSynonym synonym, SynonymModelMapper mapper, CancellationToken cancellationToken)
    {
        var viewModel = mapper.Map(synonym, SynonymTargets);

        var safeKey = synonym.Name.ToSafeKey();
        var json = JsonWriter.Serialize(viewModel);
        Bundle.AddDetail("synonym", safeKey, json);

        var outputFile = new FileInfo(Path.Combine(DataDirectory.FullName, safeKey + ".json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
