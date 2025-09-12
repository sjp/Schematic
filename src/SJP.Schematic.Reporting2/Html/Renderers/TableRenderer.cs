using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Graphviz;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class TableRenderer : ITemplateRenderer
{
    public TableRenderer(
        IIdentifierDefaults identifierDefaults,
        IHtmlFormatter formatter,
        IEnumerable<IRelationalDatabaseTable> tables,
        IReadOnlyDictionary<Identifier, ulong> rowCounts,
        DirectoryInfo exportDirectory
    )
    {
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        RowCounts = rowCounts ?? throw new ArgumentNullException(nameof(rowCounts));

        ArgumentNullException.ThrowIfNull(exportDirectory);

        ExportDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "tables"));
    }

    private IIdentifierDefaults IdentifierDefaults { get; }

    private IHtmlFormatter Formatter { get; }

    private IEnumerable<IRelationalDatabaseTable> Tables { get; }

    private IReadOnlyDictionary<Identifier, ulong> RowCounts { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var relationshipFinder = new RelationshipFinder(Tables);
        var mapper = new TableModelMapper(IdentifierDefaults, RowCounts, relationshipFinder);

        var graphvizFactory = new GraphvizExecutableFactory();
        using var graphviz = graphvizFactory.GetExecutable();

        var tableTasks = Tables.Select(async table =>
        {
            var tableModel = mapper.Map(table);
            var outputPath = Path.Combine(ExportDirectory.FullName, table.Name.ToSafeKey() + ".html");
            if (!ExportDirectory.Exists)
                ExportDirectory.Create();

            XNamespace svgNs = "http://www.w3.org/2000/svg";
            XNamespace xlinkNs = "http://www.w3.org/1999/xlink";

            var dotRenderer = new DotSvgRenderer(graphviz.DotPath);
            foreach (var diagram in tableModel.Diagrams)
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

                await using var svgFileStream = File.OpenWrite(svgFilePath);
                await doc.SaveAsync(svgFileStream, SaveOptions.DisableFormatting, cancellationToken).ConfigureAwait(false);
            }

            var renderedTable = await Formatter.RenderTemplateAsync(tableModel, cancellationToken).ConfigureAwait(false);

            var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? IdentifierDefaults.Database + " Database"
                : "Database";
            var pageTitle = table.Name.ToVisibleName() + " · Table · " + databaseName;
            var tableContainer = new Container(renderedTable, pageTitle, "../");
            var renderedPage = await Formatter.RenderTemplateAsync(tableContainer, cancellationToken).ConfigureAwait(false);

            await using (var writer = File.CreateText(outputPath))
            {
                await writer.WriteAsync(renderedPage.AsMemory(), cancellationToken).ConfigureAwait(false);
                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        });

        await Task.WhenAll(tableTasks).ConfigureAwait(false);
    }

    private const string XmlDeclaration = @"<?xml version=""1.0"" encoding=""utf-16""?>";
}