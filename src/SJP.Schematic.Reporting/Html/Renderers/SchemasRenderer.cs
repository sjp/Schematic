using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class SchemasRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new SchemaModelMapper();

        var schemaViewModels = mapper.GetSchemas(data).Select(mapper.MapSummary).ToList();
        var schemasVm = new Schemas(schemaViewModels);

        var json = context.JsonWriter.Serialize(schemasVm);
        context.Bundle.AddSummary("schemas", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "schemas.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken);
    }
}
