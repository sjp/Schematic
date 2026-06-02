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

internal sealed class ViewsRenderer : IDataRenderer
{
    public ViewsRenderer(
        IReadOnlyCollection<IDatabaseView> views,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory)
    {
        Views = views ?? throw new ArgumentNullException(nameof(views));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IReadOnlyCollection<IDatabaseView> Views { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new MainModelMapper();

        var viewViewModels = Views.Select(mapper.Map).ToList();
        var viewsVm = new Views(viewViewModels);

        var json = JsonWriter.Serialize(viewsVm);
        Bundle.AddSummary("views", json);

        var outputFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "views.json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
