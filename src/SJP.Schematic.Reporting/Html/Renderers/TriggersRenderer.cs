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

internal sealed class TriggersRenderer : IDataRenderer
{
    public TriggersRenderer(
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory)
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
        var mapper = new TriggerModelMapper();

        var triggers = Tables
            .SelectMany(t => t.Triggers.Select(tr => mapper.Map(t.Name, tr)))
            .OrderBy(static t => t.TableName, StringComparer.Ordinal)
            .ThenBy(static t => t.Name, StringComparer.Ordinal)
            .ToList();
        var triggersVm = new Triggers(triggers);

        var json = JsonWriter.Serialize(triggersVm);
        Bundle.AddSummary("triggers", json);

        var outputFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "triggers.json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
