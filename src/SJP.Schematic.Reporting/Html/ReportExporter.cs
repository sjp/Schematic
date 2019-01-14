using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;

namespace SJP.Schematic.Reporting.Html
{
    public class ReportExporter
    {
        public ReportExporter(IDbConnection connection, IRelationalDatabase database, string directory)
            : this(connection, database, new DirectoryInfo(directory))
        {
        }

        public ReportExporter(IDbConnection connection, IRelationalDatabase database, DirectoryInfo directory)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            ExportDirectory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        protected IDbConnection Connection { get; }

        protected IRelationalDatabase Database { get; }

        protected DirectoryInfo ExportDirectory { get; }

        //protected static IHtmlFormatter TemplateFormatter { get; } = new HtmlFormatter(new TemplateProvider());
        protected static IHtmlFormatter TemplateFormatter { get; } = new HtmlFormatter(new TemplateProvider());

        public async Task ExportAsync()
        {
            var renderers = GetRenderers();
            var renderTasks = renderers.Select(r => r.RenderAsync()).ToArray();
            await Task.WhenAll(renderTasks).ConfigureAwait(false);

            var assetExporter = new AssetExporter();
            await assetExporter.SaveAssetsAsync(ExportDirectory).ConfigureAwait(false);
        }

        protected IEnumerable<ITemplateRenderer> GetRenderers()
        {
            return new ITemplateRenderer[]
            {
                new ColumnsRenderer(Connection, Database, TemplateFormatter, ExportDirectory),
                new ConstraintsRenderer(Connection, Database, TemplateFormatter, ExportDirectory),
                new IndexesRenderer(Connection, Database, TemplateFormatter, ExportDirectory),
                new LintRenderer(Connection, Database, TemplateFormatter, ExportDirectory),
                new MainRenderer(Connection, Database, TemplateFormatter, ExportDirectory),
                new OrphansRenderer(Connection, Database, TemplateFormatter, ExportDirectory),
                new RelationshipsRenderer(Connection, Database, TemplateFormatter, ExportDirectory),
                new TableRenderer(Connection, Database, TemplateFormatter, ExportDirectory),
                new ViewRenderer(Connection, Database, TemplateFormatter, ExportDirectory),
                new RoutineRenderer(Connection, Database, TemplateFormatter, ExportDirectory)
            };
        }
    }
}
