using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class TableOrderingRenderer : ITemplateRenderer
    {
        public TableOrderingRenderer(
            IDatabaseDialect dialect,
            IReadOnlyCollection<IRelationalDatabaseTable> tables,
            DirectoryInfo exportDirectory)
        {
            if (tables == null || tables.AnyNull())
                throw new ArgumentNullException(nameof(tables));

            Tables = tables;

            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IDatabaseDialect Dialect { get; }

        private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            if (!ExportDirectory.Exists)
                ExportDirectory.Create();

            var cycleDetector = new CycleDetector();
            var cycles = cycleDetector.GetCyclePaths(Tables);
            var hasCycles = cycles.Count > 0;

            await ExportInsertionOrderAsync(hasCycles, cancellationToken).ConfigureAwait(false);
            await ExportDeletionOrderAsync(hasCycles, cancellationToken).ConfigureAwait(false);
        }

        private async Task ExportInsertionOrderAsync(bool hasCycles, CancellationToken cancellationToken)
        {
            var orderer = new TableRelationshipOrderer();
            var insertionOrder = orderer.GetInsertionOrder(Tables);
            var insertionOutputPath = Path.Combine(ExportDirectory.FullName, "insertion-order.sql");
            var insertionOrderDoc = BuildOrderDocument(insertionOrder);
            using var writer = File.CreateText(insertionOutputPath);
            await writer.WriteLineAsync("-- This is the insertion order for the database.".AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.WriteLineAsync("-- This may not be correct for your database or data relationships.".AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.WriteLineAsync("-- Please check before relying upon this information.".AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.WriteLineAsync().ConfigureAwait(false);

            if (hasCycles)
            {
                await writer.WriteLineAsync("-- NOTE: There are relationship cycles present in the database. This ordering is not guaranteed.".AsMemory(), cancellationToken).ConfigureAwait(false);
                await writer.WriteLineAsync().ConfigureAwait(false);
            }

            await writer.WriteAsync(insertionOrderDoc.AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
        }

        private async Task ExportDeletionOrderAsync(bool hasCycles, CancellationToken cancellationToken)
        {
            var orderer = new TableRelationshipOrderer();
            var deletionOrder = orderer.GetDeletionOrder(Tables);
            var deletionOutputPath = Path.Combine(ExportDirectory.FullName, "deletion-order.sql");
            var deletionOrderDoc = BuildOrderDocument(deletionOrder);
            using var writer = File.CreateText(deletionOutputPath);
            await writer.WriteLineAsync("-- This is the deletion order for the database.".AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.WriteLineAsync("-- This may not be correct for your database or data relationships.".AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.WriteLineAsync("-- Please check before relying upon this information.".AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.WriteLineAsync().ConfigureAwait(false);

            if (hasCycles)
            {
                await writer.WriteLineAsync("-- NOTE: There are relationship cycles present in the database. This ordering is not guaranteed.".AsMemory(), cancellationToken).ConfigureAwait(false);
                await writer.WriteLineAsync().ConfigureAwait(false);
            }

            await writer.WriteAsync(deletionOrderDoc.AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
        }

        private string BuildOrderDocument(IEnumerable<Identifier> tableNames)
        {
            if (tableNames == null)
                throw new ArgumentNullException(nameof(tableNames));

            var builder = StringBuilderCache.Acquire();

            foreach (var tableName in tableNames)
            {
                var reducedName = Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName);
                var quotedName = Dialect.QuoteName(reducedName);
                builder.AppendLine(quotedName);
            }

            return builder.GetStringAndRelease();
        }
    }
}
