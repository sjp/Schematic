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
    internal sealed class IndexesRenderer : ITemplateRenderer
    {
        public IndexesRenderer(
            IIdentifierDefaults identifierDefaults,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IRelationalDatabaseTable> tables,
            DirectoryInfo exportDirectory
        )
        {
            if (tables == null || tables.AnyNull())
                throw new ArgumentNullException(nameof(tables));

            Tables = tables;
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IIdentifierDefaults IdentifierDefaults { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            var mapper = new IndexesModelMapper();
            var allIndexes = new List<Indexes.Index>();

            foreach (var table in Tables)
            {
                var mappedIndexes = table.Indexes.Select(i => mapper.Map(table.Name, i));
                allIndexes.AddRange(mappedIndexes);
            }

            var indexes = allIndexes
                .OrderBy(i => i.TableName)
                .ThenBy(i => i.Name)
                .ToList();

            var templateParameter = new Indexes(indexes);
            var renderedIndexes = Formatter.RenderTemplate(templateParameter);

            var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? IdentifierDefaults.Database + " Database"
                : "Database";
            var pageTitle = "Indexes · " + databaseName;
            var indexesContainer = new Container(renderedIndexes, pageTitle, string.Empty);
            var renderedPage = Formatter.RenderTemplate(indexesContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "indexes.html");

            using (var writer = File.CreateText(outputPath))
            {
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}
