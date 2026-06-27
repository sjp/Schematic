using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class TableRenderer : IDataRenderer
{
    public TableRenderer(
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

        ArgumentNullException.ThrowIfNull(exportDirectory);
        DataDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "data"));
    }

    private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

    private IReadOnlyDictionary<Identifier, ulong> RowCounts { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo DataDirectory { get; }

    public Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var relationshipFinder = new RelationshipFinder(Tables);
        var mapper = new TableModelMapper(RowCounts, relationshipFinder);

        var tablesDataDirectory = new DirectoryInfo(Path.Combine(DataDirectory.FullName, "tables"));

        return RenderTaskRunner.RunAllAsync(
            Tables,
            static t => $"table '{t.Name.ToVisibleName()}'",
            (table, ct) => RenderTableAsync(table, mapper, tablesDataDirectory, ct),
            cancellationToken);
    }

    private async Task RenderTableAsync(
        IRelationalDatabaseTable table,
        TableModelMapper mapper,
        DirectoryInfo tablesDataDirectory,
        CancellationToken cancellationToken)
    {
        var tableModel = mapper.Map(table);

        // Each diagram is a graph payload that the report lays out and draws in the browser; there is
        // no SVG to render here.
        var safeKey = table.Name.ToSafeKey();
        var json = JsonWriter.Serialize(tableModel);
        Bundle.AddDetail("table", safeKey, json);

        var outputFile = new FileInfo(Path.Combine(tablesDataDirectory.FullName, safeKey + ".json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
