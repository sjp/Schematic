using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class TableOrderingRenderer : IDataRenderer
{
    public TableOrderingRenderer(IDatabaseDialect dialect)
    {
        Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
    }

    private IDatabaseDialect Dialect { get; }

    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var exportsDirectory = new DirectoryInfo(Path.Combine(context.ExportDirectory.FullName, "exports"));
        if (!exportsDirectory.Exists)
            exportsDirectory.Create();

        var cycleDetector = new CycleDetector();
        var cycles = cycleDetector.GetCyclePaths(data.Tables);
        var hasCycles = cycles.Count > 0;

        var orderer = new TableRelationshipOrderer();
        await ExportOrderAsync(exportsDirectory, "insertion", orderer.GetInsertionOrder(data.Tables), hasCycles, cancellationToken);
        await ExportOrderAsync(exportsDirectory, "deletion", orderer.GetDeletionOrder(data.Tables), hasCycles, cancellationToken);
    }

    private async Task ExportOrderAsync(DirectoryInfo exportsDirectory, string orderName, IEnumerable<Identifier> order, bool hasCycles, CancellationToken cancellationToken)
    {
        var outputPath = Path.Combine(exportsDirectory.FullName, $"{orderName}-order.sql");
        var orderDoc = BuildOrderDocument(order);
        await using var writer = File.CreateText(outputPath);
        await writer.WriteLineAsync($"-- This is the {orderName} order for the database.".AsMemory(), cancellationToken);
        await writer.WriteLineAsync("-- This may not be correct for your database or data relationships.".AsMemory(), cancellationToken);
        await writer.WriteLineAsync("-- Please check before relying upon this information.".AsMemory(), cancellationToken);
        await writer.WriteLineAsync();

        if (hasCycles)
        {
            await writer.WriteLineAsync("-- NOTE: There are relationship cycles present in the database. This ordering is not guaranteed.".AsMemory(), cancellationToken);
            await writer.WriteLineAsync();
        }

        await writer.WriteAsync(orderDoc.AsMemory(), cancellationToken);
        await writer.FlushAsync(cancellationToken);
    }

    private string BuildOrderDocument(IEnumerable<Identifier> tableNames)
    {
        ArgumentNullException.ThrowIfNull(tableNames);

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
