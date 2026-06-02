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

internal sealed class IndexesRenderer : IDataRenderer
{
    public IndexesRenderer(
        IEnumerable<IRelationalDatabaseTable> tables,
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

    private IEnumerable<IRelationalDatabaseTable> Tables { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new IndexesModelMapper();
        var allIndexes = new List<Indexes.IndexRow>();

        foreach (var table in Tables)
        {
            var mappedIndexes = table.Indexes.Select(i => mapper.Map(table.Name, i));
            allIndexes.AddRange(mappedIndexes);
        }

        var indexes = allIndexes
            .OrderBy(static i => i.TableName, StringComparer.Ordinal)
            .ThenBy(static i => i.Name, StringComparer.Ordinal)
            .ToList();

        var indexesVm = new Indexes(indexes);

        var json = JsonWriter.Serialize(indexesVm);
        Bundle.AddSummary("indexes", json);

        var outputFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "indexes.json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
