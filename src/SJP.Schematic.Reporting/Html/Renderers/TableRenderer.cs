using System;
using System.IO;
using System.Linq;
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

        var mapper = new TableModelMapper(data.Tables.Select(static t => t.Name));

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

        // The payload carries no relationship diagrams. The report derives each table's neighbourhood
        // from the schema-wide relationships graph, so a table's payload does not grow with the number
        // of tables near it.
        var safeKey = table.Name.ToSafeKey();
        var json = context.JsonWriter.Serialize(tableModel);
        context.Bundle.AddDetail("table", safeKey, json);

        var outputFile = new FileInfo(Path.Combine(tablesDataDirectory.FullName, safeKey + ".json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken);
    }
}
