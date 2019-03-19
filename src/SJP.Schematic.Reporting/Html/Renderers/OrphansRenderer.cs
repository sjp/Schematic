using System;
using System.Collections.Generic;
using System.Data;
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
            IDbConnection connection,
            IDatabaseDialect dialect,
            IIdentifierDefaults identifierDefaults,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IRelationalDatabaseTable> tables,
            DirectoryInfo exportDirectory
        )
        {
            if (tables == null || tables.AnyNull())
                throw new ArgumentNullException(nameof(tables));

            Tables = tables;

            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IDbConnection Connection { get; }

        private IDatabaseDialect Dialect { get; }

        private IIdentifierDefaults IdentifierDefaults { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var orphanedTables = Tables
                .Where(t => t.ParentKeys.Empty() && t.ChildKeys.Empty())
                .ToList();

            var mapper = new OrphansModelMapper(Connection, Dialect);
            var mappingTasks = orphanedTables.Select(t => mapper.MapAsync(t, cancellationToken)).ToArray();
            var orphanedTableViewModels = await Task.WhenAll(mappingTasks).ConfigureAwait(false);

            var templateParameter = new Orphans(orphanedTableViewModels);
            var renderedOrphans = Formatter.RenderTemplate(templateParameter);

            var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? IdentifierDefaults.Database + " Database"
                : "Database";
            var pageTitle = "Orphan Tables — " + databaseName;
            var orphansContainer = new Container(renderedOrphans, pageTitle, string.Empty);
            var renderedPage = Formatter.RenderTemplate(orphansContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "orphans.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
