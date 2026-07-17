using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html;
using SJP.Schematic.Reporting.Html.Lint;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting;

public class ReportGenerator
{
    public ReportGenerator(ISchematicConnection connection, IRelationalDatabaseProvider databaseProvider, IRelationalDatabase database, string directory)
        : this(connection, databaseProvider, database, new DirectoryInfo(directory))
    {
    }

    public ReportGenerator(ISchematicConnection connection, IRelationalDatabaseProvider databaseProvider, IRelationalDatabase database, DirectoryInfo directory)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        DatabaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        Database = database ?? throw new ArgumentNullException(nameof(database));
        ExportDirectory = directory ?? throw new ArgumentNullException(nameof(directory));
    }

    protected ISchematicConnection Connection { get; }

    protected IRelationalDatabaseProvider DatabaseProvider { get; }

    protected IRelationalDatabase Database { get; }

    protected DirectoryInfo ExportDirectory { get; }

    public async Task GenerateAsync(CancellationToken cancellationToken = default)
    {
        var (
            tables,
            views,
            sequences,
            synonyms,
            routines
        ) = await (
            Database.GetAllTables(cancellationToken),
            Database.GetAllViews(cancellationToken),
            Database.GetAllSequences(cancellationToken),
            Database.GetAllSynonyms(cancellationToken),
            Database.GetAllRoutines(cancellationToken)
        ).WhenAll();

        var rowCounts = await GetRowCountsAsync(tables, cancellationToken);

        var dbVersion = await DatabaseProvider.GetDatabaseDisplayVersionAsync(cancellationToken);

        var reportData = BuildReportData(tables, views, sequences, synonyms, routines, rowCounts, dbVersion);
        var renderContext = new RenderContext(new JsonDataWriter(), new BundleBuilder(), ExportDirectory);

        // Each renderer serializes its viewmodel(s), writes the .json file(s), and registers the
        // same payload string with the shared bundle.
        var renderers = GetRenderers();

        // Render every section, isolating failures so one bad object/section doesn't hide the rest.
        var failures = new ConcurrentBag<RenderException>();
        var renderTasks = renderers.Select(r => RenderIsolatedAsync(r, reportData, renderContext, failures, cancellationToken)).ToArray();
        await Task.WhenAll(renderTasks);

        // A partial report would silently omit objects, so surface every failure together and stop
        // before writing the shell rather than emitting a misleading report.
        RenderTaskRunner.ThrowIfAnyFailed(failures);

        // Write the file:// shim once every payload has been registered, then extract the React shell.
        var bundleFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "bundle.js"));
        await renderContext.Bundle.WriteBundleAsync(bundleFile, cancellationToken);

        var assetExporter = new AssetExporter();
        await assetExporter.SaveAssetsAsync(ExportDirectory, true, cancellationToken);
    }

    // Runs a single renderer, recording any failure (rather than throwing) so that sibling
    // renderers still run. Detail renderers report per-object failures as an AggregateException of
    // RenderExceptions; those are spliced in flat (prefixed with the renderer) instead of being
    // re-nested, so the final report is a flat list of every failed object/section. Cancellation is
    // allowed to propagate so it is not misreported as a render failure.
    private static async Task RenderIsolatedAsync(IDataRenderer renderer, ReportData data, RenderContext context, ConcurrentBag<RenderException> failures, CancellationToken cancellationToken)
    {
        var rendererName = renderer.GetType().Name;
        try
        {
            await renderer.RenderAsync(data, context, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (AggregateException aggregate)
        {
            foreach (var inner in aggregate.InnerExceptions)
            {
                if (inner is RenderException re)
                    failures.Add(new RenderException($"{rendererName}: {re.Target}", re.InnerException!));
                else
                    failures.Add(new RenderException(rendererName, inner));
            }
        }
        catch (Exception ex)
        {
            failures.Add(new RenderException(rendererName, ex));
        }
    }

    private async Task<IReadOnlyDictionary<Identifier, ulong>> GetRowCountsAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken)
    {
        var rowCountTasks = new List<Task<KeyValuePair<Identifier, ulong>>>();

        foreach (var table in tables)
            rowCountTasks.Add(GetTableRowCountAsync(table.Name, cancellationToken));

        await Task.WhenAll(rowCountTasks);

        var result = new Dictionary<Identifier, ulong>();

        foreach (var rowCountTask in rowCountTasks)
        {
            var rowCountInfo = await rowCountTask;
            result[rowCountInfo.Key] = rowCountInfo.Value;
        }

        return result;
    }

    private async Task<KeyValuePair<Identifier, ulong>> GetTableRowCountAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        var rowCount = await Connection.ConnectionFactory.GetRowCountAsync(Connection.Dialect, tableName, cancellationToken);
        return new KeyValuePair<Identifier, ulong>(tableName, rowCount);
    }

    // Assembles the full set of database objects for this run, plus the lookups derived from them,
    // into the single object every renderer's RenderAsync call receives as its "what to render".
    private ReportData BuildReportData(
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        IReadOnlyCollection<IDatabaseView> views,
        IReadOnlyCollection<IDatabaseSequence> sequences,
        IReadOnlyCollection<IDatabaseSynonym> synonyms,
        IReadOnlyCollection<IDatabaseRoutine> routines,
        IReadOnlyDictionary<Identifier, ulong> rowCounts,
        string databaseVersion
    )
    {
        ArgumentNullException.ThrowIfNull(tables);
        ArgumentNullException.ThrowIfNull(views);
        ArgumentNullException.ThrowIfNull(sequences);
        ArgumentNullException.ThrowIfNull(synonyms);
        ArgumentNullException.ThrowIfNull(routines);
        ArgumentNullException.ThrowIfNull(rowCounts);

        // Referenced-object resolution (used by view detail) maps a dependency expression to the
        // owning object's hash route, across every object type.
        var tableNames = tables.Select(static t => t.Name).ToList();
        var viewNames = views.Select(static v => v.Name).ToList();
        var sequenceNames = sequences.Select(static s => s.Name).ToList();
        var synonymNames = synonyms.Select(static s => s.Name).ToList();
        var routineNames = routines.Select(static r => r.Name).ToList();

        var dependencyProvider = Connection.Dialect.GetDependencyProvider();
        var referencedObjectTargets = new ReferencedObjectTargets(dependencyProvider, tableNames, viewNames, sequenceNames, synonymNames, routineNames);

        // Synonym target resolution maps an aliased object name to its owning object's hash route.
        var synonymTargets = new SynonymTargets(tableNames, viewNames, sequenceNames, synonymNames, routineNames);

        return new ReportData(Database, tables, views, sequences, synonyms, routines, rowCounts, databaseVersion, referencedObjectTargets, synonymTargets);
    }

    // The renderer list is fixed for every run: each renderer's constructor only takes genuine
    // collaborators (e.g. the dialect-specific linter) that wouldn't vary between calls. What to
    // render and where to write it flow in through RenderAsync instead — see IDataRenderer.
    private IEnumerable<IDataRenderer> GetRenderers()
    {
        // Lint analysis produces data/lint.json from the default HTML rule set.
        var ruleProvider = new DefaultHtmlRuleProvider();
        var rules = ruleProvider.GetRules(Connection, RuleLevel.Warning);
        var linter = new RelationalDatabaseLinter(rules);

        return
        [
            // Dashboard summary, tables list, and per-table detail.
            new MainRenderer(),
            new TablesRenderer(),
            new TableRenderer(),
            // Views & routines.
            new ViewsRenderer(),
            new ViewRenderer(),
            new RoutinesRenderer(),
            new RoutineRenderer(),
            // Sequences & synonyms.
            new SequencesRenderer(),
            new SequenceRenderer(),
            new SynonymsRenderer(),
            new SynonymRenderer(),
            // Summary-only pages: no per-object detail.
            new TriggersRenderer(),
            new ColumnsRenderer(),
            new ConstraintsRenderer(),
            new IndexesRenderer(),
            new OrphansRenderer(),
            // Lint page.
            new LintRenderer(linter),
            // Relationships & schema-wide diagrams.
            new RelationshipsRenderer(),
            // Search index.
            new SearchRenderer(),
            new TableOrderingRenderer(Connection.Dialect),
            new DbmlRenderer(),
        ];
    }
}