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
    internal sealed class OrphansRenderer : ITemplateRenderer
    {
        public OrphansRenderer(
            IIdentifierDefaults identifierDefaults,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IRelationalDatabaseTable> tables,
            IReadOnlyDictionary<Identifier, ulong> rowCounts,
            DirectoryInfo exportDirectory
        )
        {
            if (tables == null || tables.AnyNull())
                throw new ArgumentNullException(nameof(tables));

            Tables = tables;
            RowCounts = rowCounts ?? throw new ArgumentNullException(nameof(rowCounts));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IIdentifierDefaults IdentifierDefaults { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

        private IReadOnlyDictionary<Identifier, ulong> RowCounts { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            var orphanedTables = Tables
                .Where(t => t.ParentKeys.Empty() && t.ChildKeys.Empty())
                .ToList();

            var mapper = new OrphansModelMapper();
            var orphanedTableViewModels = orphanedTables
                .Select(t =>
                {
                    if (!RowCounts.TryGetValue(t.Name, out var rowCount))
                        rowCount = 0;
                    return mapper.Map(t, rowCount);
                })
                .ToList();

            var templateParameter = new Orphans(orphanedTableViewModels);
            var renderedOrphans = await Formatter.RenderTemplateAsync(templateParameter).ConfigureAwait(false);

            var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? IdentifierDefaults.Database + " Database"
                : "Database";
            var pageTitle = "Orphan Tables · " + databaseName;
            var orphansContainer = new Container(renderedOrphans, pageTitle, string.Empty);
            var renderedPage = await Formatter.RenderTemplateAsync(orphansContainer).ConfigureAwait(false);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "orphans.html");

            using (var writer = File.CreateText(outputPath))
            {
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}
