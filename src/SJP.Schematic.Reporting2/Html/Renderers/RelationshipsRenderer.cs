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

internal sealed class RelationshipsRenderer : IDataRenderer
{
    public RelationshipsRenderer(
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
        var mapper = new RelationshipsModelMapper(IdentifierDefaults);
        var viewModel = mapper.Map(Tables, RowCounts);

        var diagramsDirectory = new DirectoryInfo(Path.Combine(DataDirectory.FullName, "diagrams"));
        if (!diagramsDirectory.Exists)
            diagramsDirectory.Create();

        var graphvizFactory = new GraphvizExecutableFactory();
        using (var graphviz = graphvizFactory.GetExecutable())
        {
            var dotRenderer = new DotSvgRenderer(graphviz.DotPath);

            foreach (var diagram in viewModel.Diagrams)
            {
                var svgFilePath = Path.Combine(diagramsDirectory.FullName, diagram.ContainerId + ".svg");
                var svg = await dotRenderer.RenderToSvgAsync(diagram.Dot, cancellationToken).ConfigureAwait(false);

                var doc = XDocument.Parse(svg, LoadOptions.PreserveWhitespace);
                doc.ReplaceTitlesWithTableNames();

                // The diagram is embedded responsively via <object>, so drop the fixed dimensions.
                // In-SVG href rewriting (to hash routes) is handled by issue 13.
                var svgRoot = doc.Root!;
                svgRoot.Attribute("width")?.Remove();
                svgRoot.Attribute("height")?.Remove();

                await using var svgFileStream = File.Create(svgFilePath);
                await doc.SaveAsync(svgFileStream, SaveOptions.DisableFormatting, cancellationToken).ConfigureAwait(false);
            }
        }

        var json = JsonWriter.Serialize(viewModel);
        Bundle.AddSummary("relationships", json);

        var outputFile = new FileInfo(Path.Combine(DataDirectory.FullName, "relationships.json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
