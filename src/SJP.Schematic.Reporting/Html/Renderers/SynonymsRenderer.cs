using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class SynonymsRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new MainModelMapper();

        var synonymViewModels = data.Synonyms.Select(s => mapper.Map(s, data.SynonymTargets)).ToList();
        var synonymsVm = new Synonyms(synonymViewModels);

        var json = context.JsonWriter.Serialize(synonymsVm);
        context.Bundle.AddSummary("synonyms", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "synonyms.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
