using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class MainRenderer : ITemplateRenderer
    {
        public MainRenderer(
            IRelationalDatabase database,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IRelationalDatabaseTable> tables,
            IReadOnlyCollection<IDatabaseView> views,
            IReadOnlyCollection<IDatabaseSequence> sequences,
            IReadOnlyCollection<IDatabaseSynonym> synonyms,
            IReadOnlyCollection<IDatabaseRoutine> routines,
            IReadOnlyDictionary<Identifier, ulong> rowCounts,
            DirectoryInfo exportDirectory)
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

            Tables = tables;
            Views = views;
            Sequences = sequences;
            Synonyms = synonyms;
            Routines = routines;

            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            RowCounts = rowCounts ?? throw new ArgumentNullException(nameof(rowCounts));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IRelationalDatabase Database { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

        private IReadOnlyCollection<IDatabaseView> Views { get; }

        private IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

        private IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; }

        private IReadOnlyCollection<IDatabaseRoutine> Routines { get; }

        private IReadOnlyDictionary<Identifier, ulong> RowCounts { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            var mapper = new MainModelMapper();

            var columns = 0U;
            var constraints = 0U;
            var indexesCount = 0U;
            var tableViewModels = new List<Main.Table>();
            foreach (var table in Tables)
            {
                if (!RowCounts.TryGetValue(table.Name, out var rowCount))
                    rowCount = 0;

                var renderTable = mapper.Map(table, rowCount);

                var uniqueKeyLookup = table.GetUniqueKeyLookup();
                var uniqueKeyCount = uniqueKeyLookup.UCount();

                var checksLookup = table.GetCheckLookup();
                var checksCount = checksLookup.UCount();

                var indexesLookup = table.GetIndexLookup();
                var indexCount = indexesLookup.UCount();
                indexesCount += indexCount;

                table.PrimaryKey.IfSome(_ => constraints++);

                constraints += uniqueKeyCount;
                constraints += renderTable.ParentsCount;
                constraints += checksCount;

                columns += renderTable.ColumnCount;

                tableViewModels.Add(renderTable);
            }

            var viewViewModels = Views.Select(mapper.Map).ToList();
            columns += (uint)viewViewModels.Sum(v => v.ColumnCount);

            var sequenceViewModels = Sequences.Select(mapper.Map).ToList();

            var synonymTargets = new SynonymTargets(Tables, Views, Sequences, Synonyms, Routines);
            var synonymViewModels = Synonyms.Select(s => mapper.Map(s, synonymTargets)).ToList();

            var routineViewModels = Routines.Select(mapper.Map).ToList();

            var schemas = Tables.Select(t => t.Name)
                .Union(Views.Select(v => v.Name))
                .Union(Sequences.Select(s => s.Name))
                .Union(Synonyms.Select(s => s.Name))
                .Union(Routines.Select(r => r.Name))
                .Select(n => n.Schema)
                .Where(n => n != null)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var dbVersion = await Database.Dialect.GetDatabaseDisplayVersionAsync(cancellationToken).ConfigureAwait(false);
            var templateParameter = new Main(
                Database.IdentifierDefaults.Database,
                dbVersion,
                columns,
                constraints,
                indexesCount,
                schemas,
                tableViewModels,
                viewViewModels,
                sequenceViewModels,
                synonymViewModels,
                routineViewModels
            );

            var renderedMain = await Formatter.RenderTemplateAsync(templateParameter).ConfigureAwait(false);

            var databaseName = !Database.IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? Database.IdentifierDefaults.Database + " Database"
                : "Database";
            var pageTitle = "Home · " + databaseName;
            var mainContainer = new Container(renderedMain, pageTitle, string.Empty);
            var renderedPage = await Formatter.RenderTemplateAsync(mainContainer).ConfigureAwait(false);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "index.html");

            using var writer = File.CreateText(outputPath);
            await writer.WriteAsync(renderedPage).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
        }
    }
}
