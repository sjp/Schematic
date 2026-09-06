using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class UserDefinedTypesRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new MainModelMapper();

        var typeViewModels = data.UserDefinedTypes.Select(mapper.Map).ToList();
        var typesVm = new UserDefinedTypes(typeViewModels);

        var json = context.JsonWriter.Serialize(typesVm);
        context.Bundle.AddSummary("userDefinedTypes", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "userDefinedTypes.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken);
    }
}
