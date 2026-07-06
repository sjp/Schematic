using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class SearchRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var entries = new List<Search.SearchEntry>();

        foreach (var table in data.Tables)
        {
            var tableUrl = UrlRouter.GetTableUrl(table.Name);
            var tableName = table.Name.ToVisibleName();
            entries.Add(new Search.SearchEntry(tableName, "Table", tableUrl, null));

            foreach (var column in table.Columns)
                entries.Add(new Search.SearchEntry(column.Name.LocalName, "Column", tableUrl, tableName));
        }

        foreach (var view in data.Views)
        {
            var viewUrl = UrlRouter.GetViewUrl(view.Name);
            var viewName = view.Name.ToVisibleName();
            entries.Add(new Search.SearchEntry(viewName, "View", viewUrl, null));

            foreach (var column in view.Columns)
                entries.Add(new Search.SearchEntry(column.Name.LocalName, "Column", viewUrl, viewName));
        }

        foreach (var sequence in data.Sequences)
            entries.Add(new Search.SearchEntry(sequence.Name.ToVisibleName(), "Sequence", UrlRouter.GetSequenceUrl(sequence.Name), null));

        foreach (var synonym in data.Synonyms)
            entries.Add(new Search.SearchEntry(synonym.Name.ToVisibleName(), "Synonym", UrlRouter.GetSynonymUrl(synonym.Name), null));

        foreach (var routine in data.Routines)
            entries.Add(new Search.SearchEntry(routine.Name.ToVisibleName(), "Routine", UrlRouter.GetRoutineUrl(routine.Name), null));

        var searchVm = new Search(entries);

        var json = context.JsonWriter.Serialize(searchVm);
        context.Bundle.AddSummary("search", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "search.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
