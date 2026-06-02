using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Graphviz;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class TableRenderer : IDataRenderer
{
    public TableRenderer(
        IIdentifierDefaults identifierDefaults,
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        IReadOnlyDictionary<Identifier, ulong> rowCounts,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory
    )
    {
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        RowCounts = rowCounts ?? throw new ArgumentNullException(nameof(rowCounts));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));

        ArgumentNullException.ThrowIfNull(exportDirectory);
        DataDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "data"));
    }

    private IIdentifierDefaults IdentifierDefaults { get; }

    private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

    private IReadOnlyDictionary<Identifier, ulong> RowCounts { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo DataDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var relationshipFinder = new RelationshipFinder(Tables);
        var mapper = new TableModelMapper(IdentifierDefaults, RowCounts, relationshipFinder);

        var diagramsDirectory = new DirectoryInfo(Path.Combine(DataDirectory.FullName, "diagrams"));
        var tablesDataDirectory = new DirectoryInfo(Path.Combine(DataDirectory.FullName, "tables"));

        // Create the shared diagrams directory once, before fanning out, rather than racing on it
        // from every concurrent table task.
        if (!diagramsDirectory.Exists)
            diagramsDirectory.Create();

        var graphvizFactory = new GraphvizExecutableFactory();
        using var graphviz = graphvizFactory.GetExecutable();
        var dotRenderer = new DotSvgRenderer(graphviz.DotPath);

        // Awaited (not returned) so the graphviz executable stays alive until every table is done.
        await RenderTaskRunner.RunAllAsync(
            Tables,
            static t => $"table '{t.Name.ToVisibleName()}'",
            (table, ct) => RenderTableAsync(table, mapper, dotRenderer, diagramsDirectory, tablesDataDirectory, ct),
            cancellationToken).ConfigureAwait(false);
    }

    private async Task RenderTableAsync(
        IRelationalDatabaseTable table,
        TableModelMapper mapper,
        DotSvgRenderer dotRenderer,
        DirectoryInfo diagramsDirectory,
        DirectoryInfo tablesDataDirectory,
        CancellationToken cancellationToken)
    {
        var tableModel = mapper.Map(table);

        foreach (var diagram in tableModel.Diagrams)
        {
            var svgFilePath = Path.Combine(diagramsDirectory.FullName, diagram.ContainerId + ".svg");
            var svg = await dotRenderer.RenderToSvgAsync(diagram.Dot, cancellationToken).ConfigureAwait(false);

            var doc = XDocument.Parse(svg, LoadOptions.PreserveWhitespace);
            // Add hash-route links to nodes first; this reads the safe-key node titles that
            // ReplaceTitlesWithTableNames then overwrites with the visible table names.
            doc.RewriteTableNodeUrls();
            doc.ReplaceTitlesWithTableNames();

            // The diagram is embedded responsively via <object>, so drop the fixed dimensions.
            var svgRoot = doc.Root!;
            svgRoot.Attribute("width")?.Remove();
            svgRoot.Attribute("height")?.Remove();

            // Recolour for dark mode in-place; the embedded SVG can't see the app's theme class.
            doc.AddDarkModeStyles();

            await using var svgFileStream = File.Create(svgFilePath);
            await doc.SaveAsync(svgFileStream, SaveOptions.DisableFormatting, cancellationToken).ConfigureAwait(false);
        }

        var safeKey = table.Name.ToSafeKey();
        var json = JsonWriter.Serialize(tableModel);
        Bundle.AddDetail("table", safeKey, json);

        var outputFile = new FileInfo(Path.Combine(tablesDataDirectory.FullName, safeKey + ".json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
