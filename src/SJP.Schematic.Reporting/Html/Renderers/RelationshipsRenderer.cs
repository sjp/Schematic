using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
    internal sealed class RelationshipsRenderer : ITemplateRenderer
    {
        public RelationshipsRenderer(
            IDbConnection connection,
            IDatabaseDialect dialect,
            IIdentifierDefaults identifierDefaults,
            IHtmlFormatter formatter,
            IEnumerable<IRelationalDatabaseTable> tables,
            DirectoryInfo exportDirectory)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            Tables = tables ?? throw new ArgumentNullException(nameof(tables));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IDbConnection Connection { get; }

        private IDatabaseDialect Dialect { get; }

        private IIdentifierDefaults IdentifierDefaults { get; }

        private IHtmlFormatter Formatter { get; }

        private IEnumerable<IRelationalDatabaseTable> Tables { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var mapper = new RelationshipsModelMapper(Connection, Dialect, IdentifierDefaults);
            var viewModel = await mapper.MapAsync(Tables, cancellationToken).ConfigureAwait(false);
            var renderedRelationships = Formatter.RenderTemplate(viewModel);

            var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? IdentifierDefaults.Database + " Database"
                : "Database";
            var pageTitle = "Relationships — " + databaseName;
            var relationshipContainer = new Container(renderedRelationships, pageTitle, string.Empty);
            var renderedPage = Formatter.RenderTemplate(relationshipContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();

            XNamespace svgNs = "http://www.w3.org/2000/svg";
            XNamespace xlinkNs = "http://www.w3.org/1999/xlink";

            using (var dot = new GraphvizTemporaryExecutable())
            {
                var dotRenderer = new DotRenderer(dot.DotExecutablePath);

                foreach (var diagram in viewModel.Diagrams)
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
            }

            var outputPath = Path.Combine(ExportDirectory.FullName, "relationships.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
