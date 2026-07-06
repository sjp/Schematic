using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class ViewRenderer : IDataRenderer
{
    public Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new ViewModelMapper();
        var dataDirectory = new DirectoryInfo(Path.Combine(context.ExportDirectory.FullName, "data", "views"));

        return RenderTaskRunner.RunAllAsync(
            data.Views,
            static v => $"view '{v.Name.ToVisibleName()}'",
            (view, ct) => RenderViewAsync(view, mapper, data.ReferencedObjectTargets, context, dataDirectory, ct),
            cancellationToken);
    }

    private static async Task RenderViewAsync(
        IDatabaseView view,
        ViewModelMapper mapper,
        ReferencedObjectTargets referencedObjectTargets,
        RenderContext context,
        DirectoryInfo dataDirectory,
        CancellationToken cancellationToken)
    {
        var viewModel = mapper.Map(view, referencedObjectTargets);

        var safeKey = view.Name.ToSafeKey();
        var json = context.JsonWriter.Serialize(viewModel);
        context.Bundle.AddDetail("view", safeKey, json);

        var outputFile = new FileInfo(Path.Combine(dataDirectory.FullName, safeKey + ".json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
