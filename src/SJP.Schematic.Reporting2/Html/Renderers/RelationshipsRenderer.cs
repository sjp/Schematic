using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Graphviz;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class RelationshipsRenderer : ITemplateRenderer
{
    public RelationshipsRenderer(
        IIdentifierDefaults identifierDefaults,
        IHtmlFormatter formatter,
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        IReadOnlyDictionary<Identifier, ulong> rowCounts,
        DirectoryInfo exportDirectory)
    {
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        RowCounts = rowCounts ?? throw new ArgumentNullException(nameof(rowCounts));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IIdentifierDefaults IdentifierDefaults { get; }

    private IHtmlFormatter Formatter { get; }

    private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

    private IReadOnlyDictionary<Identifier, ulong> RowCounts { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new RelationshipsModelMapper(IdentifierDefaults);
        var viewModel = mapper.Map(Tables, RowCounts);

        if (!ExportDirectory.Exists)
            ExportDirectory.Create();

        XNamespace svgNs = "http://www.w3.org/2000/svg";
        XNamespace xlinkNs = "http://www.w3.org/1999/xlink";

        var graphvizFactory = new GraphvizExecutableFactory();
        using (var graphviz = graphvizFactory.GetExecutable())
        {
            var dotRenderer = new DotSvgRenderer(graphviz.DotPath);

            foreach (var diagram in viewModel.Diagrams)
            {
                var svgFilePath = Path.Combine(ExportDirectory.FullName, diagram.ContainerId + ".svg");
                var svg = await dotRenderer.RenderToSvgAsync(diagram.Dot, cancellationToken).ConfigureAwait(false);

                // ensure links open in new window with right attrs
                var doc = XDocument.Parse(svg, LoadOptions.PreserveWhitespace);
                doc.ReplaceTitlesWithTableNames();

                var linkNodes = doc.Descendants(svgNs + "a");
                foreach (var linkNode in linkNodes)
                {
                    linkNode.SetAttributeValue("target", "_blank");
                    linkNode.SetAttributeValue("rel", "noopener noreferrer");
                    linkNode.SetAttributeValue(xlinkNs + "show", "new");
                }

                await using (var writer = new StringWriter())
                {
                    var svgRoot = doc.Root!;
                    svgRoot.Attribute("width")?.Remove();
                    svgRoot.Attribute("height")?.Remove();

                    await svgRoot.SaveAsync(writer, SaveOptions.DisableFormatting, cancellationToken).ConfigureAwait(false);
                    var svgText = writer.ToString();
                    if (svgText.StartsWith(XmlDeclaration, StringComparison.Ordinal))
                        svgText = svgText[XmlDeclaration.Length..];
                    diagram.Svg = svgText;
                }

                // disable links, replace them with a <g>, i.e. a dummy element
                linkNodes = doc.Descendants(svgNs + "a");
                foreach (var linkNode in linkNodes)
                {
                    linkNode.RemoveAttributes();
                    linkNode.Name = svgNs + "g";
                }

                await using var svgFileStream = File.CreateText(svgFilePath);
                await doc.SaveAsync(svgFileStream, SaveOptions.DisableFormatting, cancellationToken).ConfigureAwait(false);
            }
        }

        var renderedRelationships = await Formatter.RenderTemplateAsync(viewModel, cancellationToken).ConfigureAwait(false);

        var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
            ? IdentifierDefaults.Database + " Database"
            : "Database";
        var pageTitle = "Relationships · " + databaseName;
        var relationshipContainer = new Container(renderedRelationships, pageTitle, string.Empty);
        var renderedPage = await Formatter.RenderTemplateAsync(relationshipContainer, cancellationToken).ConfigureAwait(false);

        var outputPath = Path.Combine(ExportDirectory.FullName, "relationships.html");

        await using (var writer = File.CreateText(outputPath))
        {
            await writer.WriteAsync(renderedPage.AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private const string XmlDeclaration = @"<?xml version=""1.0"" encoding=""utf-16""?>";
}