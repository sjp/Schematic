using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class TablesRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new MainModelMapper();

        var tableViewModels = new List<Main.Table>();
        foreach (var table in data.Tables)
        {
            var statistics = data.TableStatistics.TryGetValue(table.Name, out var tableStatistics)
                ? Option<ITableStatistics>.Some(tableStatistics)
                : Option<ITableStatistics>.None;

            tableViewModels.Add(mapper.Map(table, statistics));
        }

        var tablesVm = new Tables(tableViewModels);

        var json = context.JsonWriter.Serialize(tablesVm);
        context.Bundle.AddSummary("tables", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "tables.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken);
    }
}
