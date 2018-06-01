using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal class IndexesRenderer : ITemplateRenderer
    {
        public IndexesRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
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
            var mapper = new IndexesModelMapper(Database.Dialect);

            var indexes = Database.Tables
                .SelectMany(t => t.Indexes)
                .Select(mapper.Map)
                .OrderBy(i => i.TableName)
                .ThenBy(i => i.Name)
                .ToList();

            var templateParameter = new Indexes(indexes, string.Empty);
            var renderedIndexes = Formatter.RenderTemplate(templateParameter);

            var indexesContainer = new Container(renderedIndexes, Database.DatabaseName, string.Empty);
            var renderedPage = Formatter.RenderTemplate(indexesContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "indexes.html");

            File.WriteAllText(outputPath, renderedPage);
        }

        public async Task RenderAsync()
        {
            var mapper = new IndexesModelMapper(Database.Dialect);

            var tablesAsync = await Database.TablesAsync().ConfigureAwait(false);
            var tables = await tablesAsync.ToList().ConfigureAwait(false);
            var indexesTasks = tables.Select(t => t.IndexesAsync()).ToArray();
            var allIndexes = await Task.WhenAll(indexesTasks).ConfigureAwait(false);

            var indexes = allIndexes
                .SelectMany(i => i)
                .Select(mapper.Map)
                .OrderBy(i => i.TableName)
                .ThenBy(i => i.Name)
                .ToList();

            var templateParameter = new Indexes(indexes, string.Empty);
            var renderedIndexes = Formatter.RenderTemplate(templateParameter);

            var indexesContainer = new Container(renderedIndexes, Database.DatabaseName, string.Empty);
            var renderedPage = Formatter.RenderTemplate(indexesContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "indexes.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
