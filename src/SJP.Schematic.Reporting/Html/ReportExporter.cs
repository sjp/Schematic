using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.Lint;
using SJP.Schematic.Reporting.Html.Renderers;

namespace SJP.Schematic.Reporting.Html
{
    public class ReportExporter
    {
        public ReportExporter(IDbConnection connection, IRelationalDatabase database, string directory)
            : this(connection, database, new DirectoryInfo(directory))
        {
        }

        public ReportExporter(IDbConnection connection, IRelationalDatabase database, DirectoryInfo directory)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            ExportDirectory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        protected IDbConnection Connection { get; }

        protected IRelationalDatabase Database { get; }

        protected DirectoryInfo ExportDirectory { get; }

        protected static IHtmlFormatter TemplateFormatter { get; } = new HtmlFormatter(new TemplateProvider());

        public async Task ExportAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tables = await Database.GetAllTables(cancellationToken).ConfigureAwait(false);
            var views = await Database.GetAllViews(cancellationToken).ConfigureAwait(false);
            var sequences = await Database.GetAllSequences(cancellationToken).ConfigureAwait(false);
            var synonyms = await Database.GetAllSynonyms(cancellationToken).ConfigureAwait(false);
            var routines = await Database.GetAllRoutines(cancellationToken).ConfigureAwait(false);

            var renderers = GetRenderers(tables, views, sequences, synonyms, routines);
            var renderTasks = renderers.Select(r => r.RenderAsync(cancellationToken)).ToArray();
            await Task.WhenAll(renderTasks).ConfigureAwait(false);

            var assetExporter = new AssetExporter();
            await assetExporter.SaveAssetsAsync(ExportDirectory).ConfigureAwait(false);
        }

        private IEnumerable<ITemplateRenderer> GetRenderers(
            IReadOnlyCollection<IRelationalDatabaseTable> tables,
            IReadOnlyCollection<IDatabaseView> views,
            IReadOnlyCollection<IDatabaseSequence> sequences,
            IReadOnlyCollection<IDatabaseSynonym> synonyms,
            IReadOnlyCollection<IDatabaseRoutine> routines
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

            var linter = new DatabaseLinter(Connection, Database.Dialect);

            return new ITemplateRenderer[]
            {
                new ColumnsRenderer(Connection, Database.Dialect, Database.IdentifierDefaults, TemplateFormatter, tables, views, ExportDirectory),
                new ConstraintsRenderer(Database.IdentifierDefaults, TemplateFormatter, tables, ExportDirectory),
                new IndexesRenderer(Database.IdentifierDefaults, TemplateFormatter, tables, ExportDirectory),
                new LintRenderer(linter, Database.IdentifierDefaults, TemplateFormatter, tables, views, sequences, synonyms, routines, ExportDirectory),
                new MainRenderer(Connection, Database, TemplateFormatter, tables, views, sequences, synonyms, routines, ExportDirectory),
                new OrphansRenderer(Connection, Database.Dialect, Database.IdentifierDefaults, TemplateFormatter, tables, ExportDirectory),
                new RelationshipsRenderer(Connection, Database, TemplateFormatter, ExportDirectory),
                new TableRenderer(Connection, Database, TemplateFormatter, tables, ExportDirectory),
                new ViewRenderer(Connection, Database, TemplateFormatter, views, ExportDirectory),
                new RoutineRenderer(Connection, Database, TemplateFormatter, routines, ExportDirectory)
            };
        }
    }
}
