using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class SearchRenderer : IDataRenderer
{
    public SearchRenderer(
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        IReadOnlyCollection<IDatabaseView> views,
        IReadOnlyCollection<IDatabaseSequence> sequences,
        IReadOnlyCollection<IDatabaseSynonym> synonyms,
        IReadOnlyCollection<IDatabaseRoutine> routines,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory
    )
    {
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        Views = views ?? throw new ArgumentNullException(nameof(views));
        Sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
        Synonyms = synonyms ?? throw new ArgumentNullException(nameof(synonyms));
        Routines = routines ?? throw new ArgumentNullException(nameof(routines));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

    private IReadOnlyCollection<IDatabaseView> Views { get; }

    private IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

    private IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; }

    private IReadOnlyCollection<IDatabaseRoutine> Routines { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var entries = new List<Search.SearchEntry>();

        foreach (var table in Tables)
        {
            var tableUrl = UrlRouter.GetTableUrl(table.Name);
            var tableName = table.Name.ToVisibleName();
            entries.Add(new Search.SearchEntry(tableName, "Table", tableUrl, null));

            foreach (var column in table.Columns)
                entries.Add(new Search.SearchEntry(column.Name.LocalName, "Column", tableUrl, tableName));
        }

        foreach (var view in Views)
        {
            var viewUrl = UrlRouter.GetViewUrl(view.Name);
            var viewName = view.Name.ToVisibleName();
            entries.Add(new Search.SearchEntry(viewName, "View", viewUrl, null));

            foreach (var column in view.Columns)
                entries.Add(new Search.SearchEntry(column.Name.LocalName, "Column", viewUrl, viewName));
        }

        foreach (var sequence in Sequences)
            entries.Add(new Search.SearchEntry(sequence.Name.ToVisibleName(), "Sequence", UrlRouter.GetSequenceUrl(sequence.Name), null));

        foreach (var synonym in Synonyms)
            entries.Add(new Search.SearchEntry(synonym.Name.ToVisibleName(), "Synonym", UrlRouter.GetSynonymUrl(synonym.Name), null));

        foreach (var routine in Routines)
            entries.Add(new Search.SearchEntry(routine.Name.ToVisibleName(), "Routine", UrlRouter.GetRoutineUrl(routine.Name), null));

        var searchVm = new Search(entries);

        var json = JsonWriter.Serialize(searchVm);
        Bundle.AddSummary("search", json);

        var outputFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "search.json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
