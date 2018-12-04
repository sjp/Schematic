using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class MainRenderer : ITemplateRenderer
    {
        public MainRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IDbConnection Connection { get; }

        private IRelationalDatabase Database { get; }

        private IHtmlFormatter Formatter { get; }

        private DirectoryInfo ExportDirectory { get; }

        public void Render()
        {
            var tables = Database.Tables.ToList();
            var views = Database.Views.ToList();
            var sequences = Database.Sequences.ToList();
            var synonyms = Database.Synonyms.ToList();

            var mapper = new MainModelMapper(Connection, Database);

            var columns = 0U;
            var constraints = 0U;
            var indexesCount = 0U;
            var tableViewModels = new List<Main.Table>();
            foreach (var table in tables)
            {
                var renderTable = mapper.Map(table);

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

            var viewViewModels = new List<Main.View>();
            foreach (var view in views)
            {
                var renderView = mapper.Map(view);
                columns += renderView.ColumnCount;

                viewViewModels.Add(renderView);
            }

            var sequenceViewModels = sequences.Select(mapper.Map).ToList();
            var synonymViewModels = synonyms.Select(mapper.Map).ToList();

            var schemas = tables.Select(t => t.Name)
                .Union(views.Select(v => v.Name))
                .Union(sequences.Select(s => s.Name))
                .Union(synonyms.Select(s => s.Name))
                .Select(n => n.Schema)
                .Where(n => n != null)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var templateParameter = new Main(
                Database.DatabaseName,
                Database.DatabaseVersion,
                columns,
                constraints,
                indexesCount,
                schemas,
                tableViewModels,
                viewViewModels,
                sequenceViewModels,
                synonymViewModels
            );

            var renderedMain = Formatter.RenderTemplate(templateParameter);

            var mainContainer = new Container(renderedMain, Database.DatabaseName, string.Empty);
            var renderedPage = Formatter.RenderTemplate(mainContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "index.html");

            File.WriteAllText(outputPath, renderedPage);
        }

        public async Task RenderAsync()
        {
            var tables = await Database.TablesAsync().ConfigureAwait(false);
            var views = await Database.ViewsAsync().ConfigureAwait(false);
            var sequences = await Database.SequencesAsync().ConfigureAwait(false);
            var synonyms = await Database.SynonymsAsync().ConfigureAwait(false);

            var mapper = new MainModelMapper(Connection, Database);

            var columns = 0U;
            var constraints = 0U;
            var indexesCount = 0U;
            var tableViewModels = new List<Main.Table>();
            foreach (var table in tables)
            {
                var renderTable = await mapper.MapAsync(table).ConfigureAwait(false);

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

            var viewViewModels = new List<Main.View>();
            foreach (var view in views)
            {
                var renderView = await mapper.MapAsync(view).ConfigureAwait(false);
                columns += renderView.ColumnCount;

                viewViewModels.Add(renderView);
            }

            var sequenceViewModels = sequences.Select(mapper.Map).ToList();

            var synonymTasks = synonyms.Select(mapper.MapAsync).ToArray();
            var synonymViewModels = await Task.WhenAll(synonymTasks).ConfigureAwait(false);

            var schemas = tables.Select(t => t.Name)
                .Union(views.Select(v => v.Name))
                .Union(sequences.Select(s => s.Name))
                .Union(synonyms.Select(s => s.Name))
                .Select(n => n.Schema)
                .Where(n => n != null)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var templateParameter = new Main(
                Database.DatabaseName,
                Database.DatabaseVersion,
                columns,
                constraints,
                indexesCount,
                schemas,
                tableViewModels,
                viewViewModels,
                sequenceViewModels,
                synonymViewModels
            );

            var renderedMain = Formatter.RenderTemplate(templateParameter);

            var mainContainer = new Container(renderedMain, Database.DatabaseName, string.Empty);
            var renderedPage = Formatter.RenderTemplate(mainContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "index.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
