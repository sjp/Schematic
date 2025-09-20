using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html;
using SJP.Schematic.Reporting.Html.Lint;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

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

    protected static IHtmlFormatter TemplateFormatter { get; } = new HtmlFormatter(new TemplateProvider());

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
        ).WhenAll().ConfigureAwait(false);

        var rowCounts = await GetRowCountsAsync(tables, cancellationToken).ConfigureAwait(false);

        var dbVersion = await Connection.Dialect.GetDatabaseDisplayVersionAsync(Connection, cancellationToken).ConfigureAwait(false);

        var renderers = GetRenderers(tables, views, sequences, synonyms, routines, rowCounts, dbVersion);
        var renderTasks = renderers.Select(r => r.RenderAsync(cancellationToken)).ToArray();
        await Task.WhenAll(renderTasks).ConfigureAwait(false);

        var assetExporter = new AssetExporter();
        await assetExporter.SaveAssetsAsync(ExportDirectory, true, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyDictionary<Identifier, ulong>> GetRowCountsAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken)
    {
        var rowCountTasks = new List<Task<KeyValuePair<Identifier, ulong>>>();

        foreach (var table in tables)
            rowCountTasks.Add(GetTableRowCountAsync(table.Name, cancellationToken));

        await Task.WhenAll(rowCountTasks).ConfigureAwait(false);

        var result = new Dictionary<Identifier, ulong>();

        foreach (var rowCountTask in rowCountTasks)
        {
            var rowCountInfo = await rowCountTask.ConfigureAwait(false);
            result[rowCountInfo.Key] = rowCountInfo.Value;
        }

        return result;
    }

    private async Task<KeyValuePair<Identifier, ulong>> GetTableRowCountAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        var rowCount = await Connection.DbConnection.GetRowCountAsync(Connection.Dialect, tableName, cancellationToken).ConfigureAwait(false);
        return new KeyValuePair<Identifier, ulong>(tableName, rowCount);
    }

    private IEnumerable<ITemplateRenderer> GetRenderers(
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

        var ruleProvider = new ReportingRuleProvider();
        var rules = ruleProvider.GetRules(Connection, RuleLevel.Warning);
        var linter = new RelationalDatabaseLinter(rules);

        var tableNames = tables.Select(static t => t.Name).ToList();
        var viewNames = views.Select(static v => v.Name).ToList();
        var sequenceNames = sequences.Select(static s => s.Name).ToList();
        var synonymNames = synonyms.Select(static s => s.Name).ToList();
        var routineNames = routines.Select(static r => r.Name).ToList();
        var synonymTargets = new SynonymTargets(tableNames, viewNames, sequenceNames, synonymNames, routineNames);

        var dependencyProvider = Connection.Dialect.GetDependencyProvider();
        var referencedObjectTargets = new ReferencedObjectTargets(dependencyProvider, tableNames, viewNames, sequenceNames, synonymNames, routineNames);

        return
        [
            new ColumnsRenderer(Database.IdentifierDefaults, TemplateFormatter, tables, views, ExportDirectory),
            new ConstraintsRenderer(Database.IdentifierDefaults, TemplateFormatter, tables, ExportDirectory),
            new IndexesRenderer(Database.IdentifierDefaults, TemplateFormatter, tables, ExportDirectory),
            new LintRenderer(linter, Database.IdentifierDefaults, TemplateFormatter, tables, views, sequences, synonyms, routines, ExportDirectory),
            new MainRenderer(Database, TemplateFormatter, tables, views, sequences, synonyms, routines, rowCounts, databaseVersion, ExportDirectory),
            new OrphansRenderer(Database.IdentifierDefaults, TemplateFormatter, tables, rowCounts, ExportDirectory),
            new RelationshipsRenderer(Database.IdentifierDefaults, TemplateFormatter, tables, rowCounts, ExportDirectory),
            new TableRenderer(Database.IdentifierDefaults, TemplateFormatter, tables, rowCounts, ExportDirectory),
            new TriggerRenderer(TemplateFormatter, tables, ExportDirectory),
            new ViewRenderer(Database.IdentifierDefaults, TemplateFormatter, views, referencedObjectTargets, ExportDirectory),
            new SequenceRenderer(Database.IdentifierDefaults, TemplateFormatter, sequences, ExportDirectory),
            new SynonymRenderer(Database.IdentifierDefaults, TemplateFormatter, synonyms, synonymTargets, ExportDirectory),
            new RoutineRenderer(Database.IdentifierDefaults, TemplateFormatter, routines, ExportDirectory),
            new TablesRenderer(Database.IdentifierDefaults, TemplateFormatter, tables, rowCounts, ExportDirectory),
            new TriggersRenderer(Database.IdentifierDefaults, TemplateFormatter, tables, ExportDirectory),
            new ViewsRenderer(Database.IdentifierDefaults, TemplateFormatter, views, ExportDirectory),
            new SequencesRenderer(Database.IdentifierDefaults, TemplateFormatter, sequences, ExportDirectory),
            new SynonymsRenderer(Database.IdentifierDefaults, TemplateFormatter, synonyms, synonymTargets, ExportDirectory),
            new RoutinesRenderer(Database.IdentifierDefaults, TemplateFormatter, routines, ExportDirectory),
            new TableOrderingRenderer(Connection.Dialect, tables, ExportDirectory),
            new DbmlRenderer(tables, ExportDirectory),
        ];
    }
}