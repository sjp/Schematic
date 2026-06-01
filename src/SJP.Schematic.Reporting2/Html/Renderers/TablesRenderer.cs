using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class TablesRenderer : IDataRenderer
{
    public TablesRenderer(
        IEnumerable<IRelationalDatabaseTable> tables,
        IReadOnlyDictionary<Identifier, ulong> rowCounts,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory)
    {
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        RowCounts = rowCounts ?? throw new ArgumentNullException(nameof(rowCounts));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IEnumerable<IRelationalDatabaseTable> Tables { get; }

    private IReadOnlyDictionary<Identifier, ulong> RowCounts { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new MainModelMapper();

        var tableViewModels = new List<Main.Table>();
        foreach (var table in Tables)
        {
            if (!RowCounts.TryGetValue(table.Name, out var rowCount))
                rowCount = 0;

            tableViewModels.Add(mapper.Map(table, rowCount));
        }

        var tablesVm = new Tables(tableViewModels);

        var json = JsonWriter.Serialize(tablesVm);
        Bundle.AddSummary("tables", json);

        var outputFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "tables.json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
