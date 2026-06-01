using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting;

public class ReportGenerator
{
    public ReportGenerator(ISchematicConnection connection, IRelationalDatabase database, string directory)
        : this(connection, database, new DirectoryInfo(directory))
    {
    }

    public ReportGenerator(ISchematicConnection connection, IRelationalDatabase database, DirectoryInfo directory)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        Database = database ?? throw new ArgumentNullException(nameof(database));
        ExportDirectory = directory ?? throw new ArgumentNullException(nameof(directory));
    }

    protected ISchematicConnection Connection { get; }

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

        var dbVersion = await Connection.Dialect.GetDatabaseDisplayVersionAsync(Connection, cancellationToken);

        var jsonWriter = new JsonDataWriter();
        var bundle = new BundleBuilder();

        // Each renderer serializes its viewmodel(s), writes the .json file(s), and registers the
        // same payload string with the shared bundle.
        var renderers = GetRenderers(tables, views, sequences, synonyms, routines, rowCounts, dbVersion, jsonWriter, bundle);
        var renderTasks = renderers.Select(r => r.RenderAsync(cancellationToken)).ToArray();
        await Task.WhenAll(renderTasks);

        // Write the file:// shim once every payload has been registered, then extract the React shell.
        var bundleFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile, cancellationToken);

        var assetExporter = new AssetExporter();
        await assetExporter.SaveAssetsAsync(ExportDirectory, true, cancellationToken);
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
        var rowCount = await Connection.DbConnection.GetRowCountAsync(Connection.Dialect, tableName, cancellationToken);
        return new KeyValuePair<Identifier, ulong>(tableName, rowCount);
    }

    private IEnumerable<IDataRenderer> GetRenderers(
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        IReadOnlyCollection<IDatabaseView> views,
        IReadOnlyCollection<IDatabaseSequence> sequences,
        IReadOnlyCollection<IDatabaseSynonym> synonyms,
        IReadOnlyCollection<IDatabaseRoutine> routines,
        IReadOnlyDictionary<Identifier, ulong> rowCounts,
        string databaseVersion,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle
    )
    {
        ArgumentNullException.ThrowIfNull(tables);
        ArgumentNullException.ThrowIfNull(views);
        ArgumentNullException.ThrowIfNull(sequences);
        ArgumentNullException.ThrowIfNull(synonyms);
        ArgumentNullException.ThrowIfNull(routines);
        ArgumentNullException.ThrowIfNull(rowCounts);
        ArgumentNullException.ThrowIfNull(jsonWriter);
        ArgumentNullException.ThrowIfNull(bundle);

        // Non-app artifacts (.dbml/.sql) go under exports/; the app data goes under data/.
        var exportsDirectory = new DirectoryInfo(Path.Combine(ExportDirectory.FullName, "exports"));

        return
        [
            // Tables reference slice (issue 06): dashboard summary, tables list, and per-table
            // detail. Further per-type data renderers (views/sequences/... -> data/*.json) are
            // added here by issues 07-11; the SearchRenderer (-> data/search.json) slot is filled
            // in issue 12. Until then only the converted renderers run, so each milestone builds
            // and is verifiable on its own.
            new MainRenderer(Database, tables, views, sequences, synonyms, routines, databaseVersion, jsonWriter, bundle, ExportDirectory),
            new TablesRenderer(tables, rowCounts, jsonWriter, bundle, ExportDirectory),
            new TableRenderer(Database.IdentifierDefaults, tables, rowCounts, jsonWriter, bundle, ExportDirectory),
            new TableOrderingRenderer(Connection.Dialect, tables, exportsDirectory),
            new DbmlRenderer(tables, exportsDirectory),
        ];
    }
}