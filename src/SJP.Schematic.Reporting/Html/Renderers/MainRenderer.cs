using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal class MainRenderer : ITemplateRenderer
    {
        public MainRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        protected IDbConnection Connection { get; }

        protected IRelationalDatabase Database { get; }

        protected IHtmlFormatter Formatter { get; }

        protected DirectoryInfo ExportDirectory { get; }

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

                var uniqueKeyLookup = table.UniqueKey;
                var uniqueKeyCount = uniqueKeyLookup.UCount();

                var checksLookup = table.Check;
                var checksCount = checksLookup.UCount();

                var indexesLookup = table.Index;
                var indexCount = indexesLookup.UCount();
                indexesCount += indexCount;

                if (table.PrimaryKey != null)
                    constraints++;

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

            var templateParameter = new Main(
                Database.DatabaseName,
                string.Empty,
                string.Empty,
                columns,
                constraints,
                indexesCount,
                Array.Empty<string>(), // schemas
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
            var tableCollection = await Database.TablesAsync().ConfigureAwait(false);
            var viewCollection = await Database.ViewsAsync().ConfigureAwait(false);
            var sequencesCollection = await Database.SequencesAsync().ConfigureAwait(false);
            var synonymsCollection = await Database.SynonymsAsync().ConfigureAwait(false);

            var tables = await tableCollection.ToList().ConfigureAwait(false);
            var views = await viewCollection.ToList().ConfigureAwait(false);
            var sequences = await sequencesCollection.ToList().ConfigureAwait(false);
            var synonyms = await synonymsCollection.ToList().ConfigureAwait(false);

            var mapper = new MainModelMapper(Connection, Database);

            var columns = 0U;
            var constraints = 0U;
            var indexesCount = 0U;
            var tableViewModels = new List<Main.Table>();
            foreach (var table in tables)
            {
                var renderTable = await mapper.MapAsync(table).ConfigureAwait(false);

                var uniqueKeyLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
                var uniqueKeyCount = uniqueKeyLookup.UCount();

                var checksLookup = await table.CheckAsync().ConfigureAwait(false);
                var checksCount = checksLookup.UCount();

                var indexesLookup = await table.IndexAsync().ConfigureAwait(false);
                var indexCount = indexesLookup.UCount();
                indexesCount += indexCount;

                var primaryKey = await table.PrimaryKeyAsync().ConfigureAwait(false);
                if (primaryKey != null)
                    constraints++;

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

            var templateParameter = new Main(
                Database.DatabaseName,
                string.Empty,
                string.Empty,
                columns,
                constraints,
                indexesCount,
                Array.Empty<string>(), // schemas
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
