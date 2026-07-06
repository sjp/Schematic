using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class ConstraintsRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var primaryKeys = data.Tables.SelectMany(static t => t.PrimaryKey.Select(pk => new { TableName = t.Name, PrimaryKey = pk }));
        var uniqueKeys = data.Tables.SelectMany(static t => t.UniqueKeys.Select(uk => new { TableName = t.Name, UniqueKey = uk }));
        var foreignKeys = data.Tables.SelectMany(static t => t.ParentKeys);
        var checkConstraints = data.Tables.SelectMany(static t => t.Checks.Select(ck => new { TableName = t.Name, Check = ck }));

        var mapper = new ConstraintsModelMapper();

        var primaryKeyViewModels = primaryKeys
            .Select(pk => mapper.MapPrimaryKey(pk.TableName, pk.PrimaryKey))
            .OrderBy(static pk => pk.TableName, StringComparer.Ordinal)
            .ToList();
        var uniqueKeyViewModels = uniqueKeys
            .Select(uk => mapper.MapUniqueKey(uk.TableName, uk.UniqueKey))
            .OrderBy(static uk => uk.TableName, StringComparer.Ordinal)
            .ToList();
        var foreignKeyViewModels = foreignKeys
            .Select(mapper.MapForeignKey)
            .OrderBy(static fk => fk.TableName, StringComparer.Ordinal)
            .ToList();
        var checkConstraintViewModels = checkConstraints
            .Select(ck => mapper.MapCheckConstraint(ck.TableName, ck.Check))
            .OrderBy(static ck => ck.TableName, StringComparer.Ordinal)
            .ToList();

        var constraintsVm = new Constraints(
            primaryKeyViewModels,
            uniqueKeyViewModels,
            foreignKeyViewModels,
            checkConstraintViewModels
        );

        var json = context.JsonWriter.Serialize(constraintsVm);
        context.Bundle.AddSummary("constraints", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "constraints.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
