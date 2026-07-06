using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class ViewsRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new MainModelMapper();

        var viewViewModels = data.Views.Select(mapper.Map).ToList();
        var viewsVm = new Views(viewViewModels);

        var json = context.JsonWriter.Serialize(viewsVm);
        context.Bundle.AddSummary("views", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "views.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
