using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class IndexesRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new IndexesModelMapper();
        var allIndexes = new List<Indexes.IndexRow>();

        foreach (var table in data.Tables)
        {
            var mappedIndexes = table.Indexes.Select(i => mapper.Map(table.Name, i));
            allIndexes.AddRange(mappedIndexes);
        }

        var indexes = allIndexes
            .OrderBy(static i => i.TableName, StringComparer.Ordinal)
            .ThenBy(static i => i.Name, StringComparer.Ordinal)
            .ToList();

        var indexesVm = new Indexes(indexes);

        var json = context.JsonWriter.Serialize(indexesVm);
        context.Bundle.AddSummary("indexes", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "indexes.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
