using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class OrphansRenderer : IDataRenderer
{
    public OrphansRenderer(
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        IReadOnlyDictionary<Identifier, ulong> rowCounts,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory
    )
    {
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        RowCounts = rowCounts ?? throw new ArgumentNullException(nameof(rowCounts));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

    private IReadOnlyDictionary<Identifier, ulong> RowCounts { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var orphanedTables = Tables
            .Where(static t => t.ParentKeys.Empty() && t.ChildKeys.Empty())
            .ToList();

        var mapper = new OrphansModelMapper();
        var orphanedTableViewModels = orphanedTables.ConvertAll(t =>
        {
            if (!RowCounts.TryGetValue(t.Name, out var rowCount))
                rowCount = 0;
            return mapper.Map(t, rowCount);
        });

        var orphansVm = new Orphans(orphanedTableViewModels);

        var json = JsonWriter.Serialize(orphansVm);
        Bundle.AddSummary("orphans", json);

        var outputFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "orphans.json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
