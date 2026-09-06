using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class SchemaRenderer : IDataRenderer
{
    public Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new SchemaModelMapper();
        var dataDirectory = new DirectoryInfo(Path.Combine(context.ExportDirectory.FullName, "data", "schemas"));

        return RenderTaskRunner.RunAllAsync(
            mapper.GetSchemas(data),
            static s => $"schema '{s.Name.LocalName}'",
            (schema, ct) => RenderSchemaAsync(schema, mapper, context, dataDirectory, ct),
            cancellationToken);
    }

    private static async Task RenderSchemaAsync(
        SchemaModelMapper.SchemaObjects schema,
        SchemaModelMapper mapper,
        RenderContext context,
        DirectoryInfo dataDirectory,
        CancellationToken cancellationToken)
    {
        var viewModel = mapper.MapDetail(schema);

        var safeKey = schema.Name.ToSafeKey();
        var json = context.JsonWriter.Serialize(viewModel);
        context.Bundle.AddDetail("schema", safeKey, json);

        var outputFile = new FileInfo(Path.Combine(dataDirectory.FullName, safeKey + ".json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken);
    }
}
