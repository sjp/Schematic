using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class IndexesRenderer : ITemplateRenderer
    {
        public IndexesRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
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

        public async Task RenderAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tables = await Database.GetAllTables(cancellationToken).ConfigureAwait(false);

            var mapper = new IndexesModelMapper();
            var allIndexes = new List<Indexes.Index>();

            foreach (var table in tables)
            {
                var mappedIndexes = table.Indexes.Select(i => mapper.Map(table.Name, i));
                allIndexes.AddRange(mappedIndexes);
            }

            var indexes = allIndexes
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
