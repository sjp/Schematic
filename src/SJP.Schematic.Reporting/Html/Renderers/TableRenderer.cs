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

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class TableRenderer : ITemplateRenderer
    {
        public TableRenderer(
            IIdentifierDefaults identifierDefaults,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IRelationalDatabaseTable> tables,
            IReadOnlyDictionary<Identifier, ulong> rowCounts,
            DirectoryInfo exportDirectory
        )
        {
            if (tables == null || tables.AnyNull())
                throw new ArgumentNullException(nameof(tables));

            Tables = tables;

            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            RowCounts = rowCounts ?? throw new ArgumentNullException(nameof(rowCounts));

            if (exportDirectory == null)
                throw new ArgumentNullException(nameof(exportDirectory));

            ExportDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "tables"));
        }

        private IIdentifierDefaults IdentifierDefaults { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

        private IReadOnlyDictionary<Identifier, ulong> RowCounts { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var relationshipFinder = new RelationshipFinder(Tables);
            var mapper = new TableModelMapper(IdentifierDefaults, RowCounts, relationshipFinder);

            using (var dot = new GraphvizTemporaryExecutable())
            {
                var tableTasks = Tables.Select(async table =>
                {
                    var tableModel = mapper.Map(table);
                    var renderedTable = Formatter.RenderTemplate(tableModel);

                    var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                        ? IdentifierDefaults.Database + " Database"
                        : "Database";
                    var pageTitle = table.Name.ToVisibleName() + " — Table — " + databaseName;
                    var tableContainer = new Container(renderedTable, pageTitle, "../");
                    var renderedPage = Formatter.RenderTemplate(tableContainer);

                    var outputPath = Path.Combine(ExportDirectory.FullName, table.Name.ToSafeKey() + ".html");
                    if (!ExportDirectory.Exists)
                        ExportDirectory.Create();

                    XNamespace svgNs = "http://www.w3.org/2000/svg";
                    XNamespace xlinkNs = "http://www.w3.org/1999/xlink";

                    var dotRenderer = new DotRenderer(dot.DotExecutablePath);
                    foreach (var diagram in tableModel.Diagrams)
                    {
                        var svgFilePath = Path.Combine(ExportDirectory.FullName, diagram.ContainerId + ".svg");
                        var svgLinkedFilePath = Path.Combine(ExportDirectory.FullName, diagram.ContainerId + "-linked.svg");
                        var svg = dotRenderer.RenderToSvg(diagram.Dot);

                        // ensure links open in new window with right attrs
                        var doc = XDocument.Parse(svg);
                        var linkNodes = doc.Descendants(svgNs + "a");
                        foreach (var linkNode in linkNodes)
                        {
                            linkNode.SetAttributeValue("target", "_blank");
                            linkNode.SetAttributeValue("rel", "noopener noreferrer");
                            linkNode.SetAttributeValue(xlinkNs + "show", "new");
                        }

                        using (var writer = File.CreateText(svgLinkedFilePath))
                            doc.Save(writer, SaveOptions.DisableFormatting);

                        // disable links, replace them with a <g>, i.e. a dummy element
                        linkNodes = doc.Descendants(svgNs + "a");
                        foreach (var linkNode in linkNodes)
                        {
                            linkNode.RemoveAttributes();
                            linkNode.Name = svgNs + "g";
                        }

                        using (var writer = File.CreateText(svgFilePath))
                            doc.Save(writer, SaveOptions.DisableFormatting);
                    }

                    using (var writer = File.CreateText(outputPath))
                    {
                        await writer.WriteAsync(renderedPage).ConfigureAwait(false);
                        await writer.FlushAsync().ConfigureAwait(false);
                    }
                });

                await Task.WhenAll(tableTasks).ConfigureAwait(false);
            }
        }
    }
}
