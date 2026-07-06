using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class TriggersRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new TriggerModelMapper();

        var triggers = data.Tables
            .SelectMany(t => t.Triggers.Select(tr => mapper.Map(t.Name, tr)))
            .OrderBy(static t => t.TableName, StringComparer.Ordinal)
            .ThenBy(static t => t.Name, StringComparer.Ordinal)
            .ToList();
        var triggersVm = new Triggers(triggers);

        var json = context.JsonWriter.Serialize(triggersVm);
        context.Bundle.AddSummary("triggers", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "triggers.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
