using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class TablesRenderer : ITemplateRenderer
    {
        public TablesRenderer(
            IIdentifierDefaults identifierDefaults,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IRelationalDatabaseTable> tables,
            IReadOnlyDictionary<Identifier, ulong> rowCounts,
            DirectoryInfo exportDirectory)
        {
            if (tables == null || tables.AnyNull())
                throw new ArgumentNullException(nameof(tables));

            Tables = tables;

            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            RowCounts = rowCounts ?? throw new ArgumentNullException(nameof(rowCounts));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IIdentifierDefaults IdentifierDefaults { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

        private IReadOnlyDictionary<Identifier, ulong> RowCounts { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            var mapper = new MainModelMapper();

            var tableViewModels = new List<Main.Table>();
            foreach (var table in Tables)
            {
                if (!RowCounts.TryGetValue(table.Name, out var rowCount))
                    rowCount = 0;

                var renderTable = mapper.Map(table, rowCount);
                tableViewModels.Add(renderTable);
            }

            var tablesVm = new Tables(tableViewModels);
            var renderedMain = await Formatter.RenderTemplateAsync(tablesVm).ConfigureAwait(false);

            var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? IdentifierDefaults.Database + " Database"
                : "Database";
            var pageTitle = "Tables · " + databaseName;
            var mainContainer = new Container(renderedMain, pageTitle, string.Empty);
            var renderedPage = await Formatter.RenderTemplateAsync(mainContainer).ConfigureAwait(false);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "tables.html");

            using var writer = File.CreateText(outputPath);
            await writer.WriteAsync(renderedPage).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
        }
    }
}
