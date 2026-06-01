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

internal sealed class ColumnsRenderer : IDataRenderer
{
    public ColumnsRenderer(
        IEnumerable<IRelationalDatabaseTable> tables,
        IEnumerable<IDatabaseView> views,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory)
    {
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        Views = views ?? throw new ArgumentNullException(nameof(views));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IEnumerable<IRelationalDatabaseTable> Tables { get; }

    private IEnumerable<IDatabaseView> Views { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new ColumnsModelMapper();

        var tableColumns = Tables.SelectMany(mapper.Map);
        var viewColumns = Views.SelectMany(mapper.Map);

        var orderedColumns = tableColumns
            .Concat(viewColumns)
            .OrderBy(static c => c.Name, StringComparer.Ordinal)
            .ThenBy(static c => c.Ordinal)
            .ToList();

        var columnsVm = new Columns(orderedColumns);

        var json = JsonWriter.Serialize(columnsVm);
        Bundle.AddSummary("columns", json);

        var outputFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "columns.json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
