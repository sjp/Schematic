using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class TableRenderer : IDataRenderer
{
    public Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var relationshipFinder = new RelationshipFinder(data.Tables);
        var mapper = new TableModelMapper(data.RowCounts, relationshipFinder);

        var tablesDataDirectory = new DirectoryInfo(Path.Combine(context.ExportDirectory.FullName, "data", "tables"));

        return RenderTaskRunner.RunAllAsync(
            data.Tables,
            static t => $"table '{t.Name.ToVisibleName()}'",
            (table, ct) => RenderTableAsync(table, mapper, context, tablesDataDirectory, ct),
            cancellationToken);
    }

    private static async Task RenderTableAsync(
        IRelationalDatabaseTable table,
        TableModelMapper mapper,
        RenderContext context,
        DirectoryInfo tablesDataDirectory,
        CancellationToken cancellationToken)
    {
        var tableModel = mapper.Map(table);

        // Each diagram is a graph payload that the report lays out and draws in the browser; there is
        // no SVG to render here.
        var safeKey = table.Name.ToSafeKey();
        var json = context.JsonWriter.Serialize(tableModel);
        context.Bundle.AddDetail("table", safeKey, json);

        var outputFile = new FileInfo(Path.Combine(tablesDataDirectory.FullName, safeKey + ".json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
