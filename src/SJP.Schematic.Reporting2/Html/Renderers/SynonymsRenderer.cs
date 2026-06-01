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

internal sealed class SynonymsRenderer : IDataRenderer
{
    public SynonymsRenderer(
        IEnumerable<IDatabaseSynonym> synonyms,
        SynonymTargets synonymTargets,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory)
    {
        Synonyms = synonyms ?? throw new ArgumentNullException(nameof(synonyms));
        SynonymTargets = synonymTargets ?? throw new ArgumentNullException(nameof(synonymTargets));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IEnumerable<IDatabaseSynonym> Synonyms { get; }

    private SynonymTargets SynonymTargets { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new MainModelMapper();

        var synonymViewModels = Synonyms.Select(s => mapper.Map(s, SynonymTargets)).ToList();
        var synonymsVm = new Synonyms(synonymViewModels);

        var json = JsonWriter.Serialize(synonymsVm);
        Bundle.AddSummary("synonyms", json);

        var outputFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "synonyms.json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
