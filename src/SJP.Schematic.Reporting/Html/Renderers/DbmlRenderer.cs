using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Dbml;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class DbmlRenderer : ITemplateRenderer
    {
        public DbmlRenderer(
            IReadOnlyCollection<IRelationalDatabaseTable> tables,
            DirectoryInfo exportDirectory)
        {
            if (tables == null || tables.AnyNull())
                throw new ArgumentNullException(nameof(tables));

            Tables = tables;

            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            if (!ExportDirectory.Exists)
                ExportDirectory.Create();

            var formatter = new DbmlFormatter();
            var dbmlDocument = formatter.RenderTables(Tables);
            var dbmlOutputPath = Path.Combine(ExportDirectory.FullName, "relationships.dbml");
            using (var writer = File.CreateText(dbmlOutputPath))
            {
                await writer.WriteAsync(dbmlDocument).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}
