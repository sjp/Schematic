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
    public TableOrderingRenderer(
        IDatabaseDialect dialect,
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        DirectoryInfo exportDirectory)
    {
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
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

        var orderer = new TableRelationshipOrderer();
        await ExportOrderAsync("insertion", orderer.GetInsertionOrder(Tables), hasCycles, cancellationToken);
        await ExportOrderAsync("deletion", orderer.GetDeletionOrder(Tables), hasCycles, cancellationToken);
    }

    private async Task ExportOrderAsync(string orderName, IEnumerable<Identifier> order, bool hasCycles, CancellationToken cancellationToken)
    {
        var outputPath = Path.Combine(ExportDirectory.FullName, $"{orderName}-order.sql");
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