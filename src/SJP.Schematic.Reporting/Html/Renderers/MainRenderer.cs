using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class MainRenderer : IDataRenderer
{
    public MainRenderer(
        IRelationalDatabase database,
        IEnumerable<IRelationalDatabaseTable> tables,
        IEnumerable<IDatabaseView> views,
        IEnumerable<IDatabaseSequence> sequences,
        IEnumerable<IDatabaseSynonym> synonyms,
        IEnumerable<IDatabaseRoutine> routines,
        string dbVersion,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory)
    {
        Database = database ?? throw new ArgumentNullException(nameof(database));
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        Views = views ?? throw new ArgumentNullException(nameof(views));
        Sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
        Synonyms = synonyms ?? throw new ArgumentNullException(nameof(synonyms));
        Routines = routines ?? throw new ArgumentNullException(nameof(routines));
        DatabaseDisplayVersion = dbVersion;
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IRelationalDatabase Database { get; }

    private IEnumerable<IRelationalDatabaseTable> Tables { get; }

    private IEnumerable<IDatabaseView> Views { get; }

    private IEnumerable<IDatabaseSequence> Sequences { get; }

    private IEnumerable<IDatabaseSynonym> Synonyms { get; }

    private IEnumerable<IDatabaseRoutine> Routines { get; }

    private string DatabaseDisplayVersion { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var columns = 0U;
        var constraints = 0U;
        var indexesCount = 0U;
        var tablesCount = 0U;

        var tableNames = new List<Identifier>();
        foreach (var table in Tables)
        {
            tablesCount++;

            var uniqueKeyCount = table.GetUniqueKeyLookup().UCount();
            var checksCount = table.GetCheckLookup().UCount();
            indexesCount += table.GetIndexLookup().UCount();

            await table.PrimaryKey.IfSomeAsync(_ => constraints++).ConfigureAwait(false);

            constraints += uniqueKeyCount;
            constraints += table.ParentKeys.UCount();
            constraints += checksCount;

            columns += table.Columns.UCount();

            tableNames.Add(table.Name);
        }

        var viewNames = new List<Identifier>();
        var viewsCount = 0U;
        foreach (var view in Views)
        {
            viewsCount++;
            columns += view.Columns.UCount();
            viewNames.Add(view.Name);
        }

        var sequenceNames = Sequences.Select(static s => s.Name).ToList();
        var routineNames = Routines.Select(static r => r.Name).ToList();
        var synonymNames = Synonyms.Select(static s => s.Name).ToList();

        var schemas = tableNames
            .Union(viewNames)
            .Union(sequenceNames)
            .Union(synonymNames)
            .Union(routineNames)
            .Select(static n => n.Schema)
            .Where(static n => n != null)
            .Distinct(StringComparer.Ordinal)
            .Select(static s => s!)
            .Order(StringComparer.Ordinal)
            .ToList();

        var mainModel = new Main(
            Database.IdentifierDefaults.Database,
            DatabaseDisplayVersion ?? string.Empty,
            columns,
            constraints,
            indexesCount,
            schemas,
            tablesCount,
            viewsCount,
            (uint)sequenceNames.Count,
            (uint)synonymNames.Count,
            (uint)routineNames.Count
        );

        var json = JsonWriter.Serialize(mainModel);
        Bundle.AddSummary("main", json);

        var outputFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "main.json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
