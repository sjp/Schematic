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
            (userDefinedType, ct) => RenderUserDefinedTypeAsync(userDefinedType, mapper, data.UserDefinedTypeTargets, context, dataDirectory, ct),
            cancellationToken);
    }

    private static async Task RenderUserDefinedTypeAsync(
        IDatabaseUserDefinedType userDefinedType,
        UserDefinedTypeModelMapper mapper,
        UserDefinedTypeTargets userDefinedTypeTargets,
        RenderContext context,
        DirectoryInfo dataDirectory,
        CancellationToken cancellationToken)
    {
        var viewModel = mapper.Map(userDefinedType, userDefinedTypeTargets);

        var safeKey = userDefinedType.Name.ToSafeKey();
        var outputFile = new FileInfo(Path.Combine(dataDirectory.FullName, safeKey + ".json"));
        await context.JsonWriter.SerializeToFileAsync(outputFile, viewModel, cancellationToken);
        context.Bundle.AddDetail("userDefinedType", safeKey, outputFile);
    }
}
