using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class ConstraintsRenderer : IDataRenderer
{
    public ConstraintsRenderer(
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory
    )
    {
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var primaryKeys = Tables.SelectMany(static t => t.PrimaryKey.Select(pk => new { TableName = t.Name, PrimaryKey = pk }));
        var uniqueKeys = Tables.SelectMany(static t => t.UniqueKeys.Select(uk => new { TableName = t.Name, UniqueKey = uk }));
        var foreignKeys = Tables.SelectMany(static t => t.ParentKeys);
        var checkConstraints = Tables.SelectMany(static t => t.Checks.Select(ck => new { TableName = t.Name, Check = ck }));

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

        var json = JsonWriter.Serialize(constraintsVm);
        Bundle.AddSummary("constraints", json);

        var outputFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "constraints.json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
