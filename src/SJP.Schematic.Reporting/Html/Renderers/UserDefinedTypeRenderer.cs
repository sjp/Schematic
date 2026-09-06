using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class UserDefinedTypeRenderer : IDataRenderer
{
    public Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var mapper = new UserDefinedTypeModelMapper();
        var dataDirectory = new DirectoryInfo(Path.Combine(context.ExportDirectory.FullName, "data", "userDefinedTypes"));

        return RenderTaskRunner.RunAllAsync(
            data.UserDefinedTypes,
            static t => $"user-defined type '{t.Name.ToVisibleName()}'",
            (userDefinedType, ct) => RenderUserDefinedTypeAsync(userDefinedType, mapper, context, dataDirectory, ct),
            cancellationToken);
    }

    private static async Task RenderUserDefinedTypeAsync(
        IDatabaseUserDefinedType userDefinedType,
        UserDefinedTypeModelMapper mapper,
        RenderContext context,
        DirectoryInfo dataDirectory,
        CancellationToken cancellationToken)
    {
        var viewModel = mapper.Map(userDefinedType);

        var safeKey = userDefinedType.Name.ToSafeKey();
        var json = context.JsonWriter.Serialize(viewModel);
        context.Bundle.AddDetail("userDefinedType", safeKey, json);

        var outputFile = new FileInfo(Path.Combine(dataDirectory.FullName, safeKey + ".json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken);
    }
}
