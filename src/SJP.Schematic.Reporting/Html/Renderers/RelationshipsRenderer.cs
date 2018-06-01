using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal class RelationshipsRenderer : ITemplateRenderer
    {
        public RelationshipsRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        protected IDbConnection Connection { get; }

        protected IRelationalDatabase Database { get; }

        protected IHtmlFormatter Formatter { get; }

        protected DirectoryInfo ExportDirectory { get; }

        public void Render()
        {
            var mapper = new RelationshipsModelMapper(Connection);
            var templateParameter = mapper.Map(Database);
            var renderedRelationships = Formatter.RenderTemplate(templateParameter);

            var relationshipContainer = new Container(renderedRelationships, Database.DatabaseName, string.Empty);
            var renderedPage = Formatter.RenderTemplate(relationshipContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "relationships.html");

            File.WriteAllText(outputPath, renderedPage);
        }

        public async Task RenderAsync()
        {
            var mapper = new RelationshipsModelMapper(Connection);
            var templateParameter = await mapper.MapAsync(Database).ConfigureAwait(false);
            var renderedRelationships = Formatter.RenderTemplate(templateParameter);

            var relationshipContainer = new Container(renderedRelationships, Database.DatabaseName, string.Empty);
            var renderedPage = Formatter.RenderTemplate(relationshipContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "relationships.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
