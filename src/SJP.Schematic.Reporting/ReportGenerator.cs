using System;
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

namespace SJP.Schematic.Reporting
{
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
            var tablesTask = Database.GetAllTables(cancellationToken).ToListAsync(cancellationToken).AsTask();
            var viewsTask = Database.GetAllViews(cancellationToken).ToListAsync(cancellationToken).AsTask();
            var sequencesTask = Database.GetAllSequences(cancellationToken).ToListAsync(cancellationToken).AsTask();
            var synonymsTask = Database.GetAllSynonyms(cancellationToken).ToListAsync(cancellationToken).AsTask();
            var routinesTask = Database.GetAllRoutines(cancellationToken).ToListAsync(cancellationToken).AsTask();

            await Task.WhenAll(tablesTask, viewsTask, sequencesTask, synonymsTask, routinesTask).ConfigureAwait(false);

            var tables = await tablesTask.ConfigureAwait(false);
            var views = await viewsTask.ConfigureAwait(false);
            var sequences = await sequencesTask.ConfigureAwait(false);
            var synonyms = await synonymsTask.ConfigureAwait(false);
            var routines = await routinesTask.ConfigureAwait(false);

            var rowCounts = await GetRowCountsAsync(tables, cancellationToken).ConfigureAwait(false);

            var dbVersion = await Connection.Dialect.GetDatabaseDisplayVersionAsync(Connection, cancellationToken).ConfigureAwait(false);

            var renderers = GetRenderers(tables, views, sequences, synonyms, routines, rowCounts, dbVersion);
            var renderTasks = renderers.Select(r => r.RenderAsync(cancellationToken)).ToArray();
            await Task.WhenAll(renderTasks).ConfigureAwait(false);

            var assetExporter = new AssetExporter();
            await assetExporter.SaveAssetsAsync(ExportDirectory).ConfigureAwait(false);
        }

        private async Task<IReadOnlyDictionary<Identifier, ulong>> GetRowCountsAsync(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken)
        {
            var rowCountTasks = new List<Task<ulong>>();

            foreach (var table in tables)
                rowCountTasks.Add(Connection.DbConnection.GetRowCountAsync(Connection.Dialect, table.Name, cancellationToken));

            await Task.WhenAll(rowCountTasks).ConfigureAwait(false);

            var result = new Dictionary<Identifier, ulong>();

            var index = 0;
            foreach (var table in tables)
            {
                result[table.Name] = await rowCountTasks[index].ConfigureAwait(false);
                index++;
            }

            return result;
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
            if (tables == null || tables.AnyNull())
                throw new ArgumentNullException(nameof(tables));
            if (views == null || views.AnyNull())
                throw new ArgumentNullException(nameof(views));
            if (sequences == null || sequences.AnyNull())
                throw new ArgumentNullException(nameof(sequences));
            if (synonyms == null || synonyms.AnyNull())
                throw new ArgumentNullException(nameof(synonyms));
            if (routines == null || routines.AnyNull())
                throw new ArgumentNullException(nameof(routines));
            if (rowCounts == null)
                throw new ArgumentNullException(nameof(rowCounts));

            var ruleProvider = new ReportingRuleProvider();
            var rules = ruleProvider.GetRules(Connection, RuleLevel.Warning);
            var linter = new RelationalDatabaseLinter(rules);
            var synonymTargets = new SynonymTargets(tables, views, sequences, synonyms, routines);

            var dependencyProvider = Connection.Dialect.GetDependencyProvider();
            var referencedObjectTargets = new ReferencedObjectTargets(dependencyProvider, tables, views, sequences, synonyms, routines);

            return new ITemplateRenderer[]
            {
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
                new DbmlRenderer(tables, ExportDirectory)
            };
        }
    }
}
