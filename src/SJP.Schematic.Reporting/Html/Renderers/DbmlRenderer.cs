using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Dbml;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class DbmlRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var exportsDirectory = new DirectoryInfo(Path.Combine(context.ExportDirectory.FullName, "exports"));
        if (!exportsDirectory.Exists)
            exportsDirectory.Create();

        var formatter = new DbmlFormatter();
        var dbmlDocument = formatter.RenderTables(data.Tables);
        var dbmlOutputPath = Path.Combine(exportsDirectory.FullName, "relationships.dbml");
        await using var writer = File.CreateText(dbmlOutputPath);
        await writer.WriteAsync(dbmlDocument.AsMemory(), cancellationToken);
        await writer.FlushAsync(cancellationToken);
    }
}
