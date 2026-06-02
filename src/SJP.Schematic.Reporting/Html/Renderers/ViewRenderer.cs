using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class ViewRenderer : IDataRenderer
{
    public ViewRenderer(
        IEnumerable<IDatabaseView> views,
        ReferencedObjectTargets referencedObjectTargets,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory
    )
    {
        Views = views ?? throw new ArgumentNullException(nameof(views));
        ReferencedObjectTargets = referencedObjectTargets ?? throw new ArgumentNullException(nameof(referencedObjectTargets));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));

        ArgumentNullException.ThrowIfNull(exportDirectory);
        DataDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "data", "views"));
    }

    private IEnumerable<IDatabaseView> Views { get; }

    private ReferencedObjectTargets ReferencedObjectTargets { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo DataDirectory { get; }

    public Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new ViewModelMapper();

        return RenderTaskRunner.RunAllAsync(
            Views,
            static v => $"view '{v.Name.ToVisibleName()}'",
            (view, ct) => RenderViewAsync(view, mapper, ct),
            cancellationToken);
    }

    private async Task RenderViewAsync(IDatabaseView view, ViewModelMapper mapper, CancellationToken cancellationToken)
    {
        var viewModel = mapper.Map(view, ReferencedObjectTargets);

        var safeKey = view.Name.ToSafeKey();
        var json = JsonWriter.Serialize(viewModel);
        Bundle.AddDetail("view", safeKey, json);

        var outputFile = new FileInfo(Path.Combine(DataDirectory.FullName, safeKey + ".json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
