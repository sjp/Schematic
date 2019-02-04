using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
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
            var templateParameter = await mapper.MapAsync(Tables, cancellationToken).ConfigureAwait(false);
            var renderedRelationships = Formatter.RenderTemplate(templateParameter);

            var relationshipContainer = new Container(renderedRelationships, IdentifierDefaults.Database, string.Empty);
            var renderedPage = Formatter.RenderTemplate(relationshipContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "relationships.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
