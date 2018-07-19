using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal class OrphansRenderer : ITemplateRenderer
    {
        public OrphansRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
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
            var orphanedTables = Database.Tables
                .Where(t => t.ParentKeys.Empty() && t.ChildKeys.Empty())
                .ToList();

            var mapper = new OrphansModelMapper(Connection, Database.Dialect);
            var orphanedTableViewModels = orphanedTables
                .Select(mapper.Map)
                .OrderBy(vm => vm.Name)
                .ToList();

            var templateParameter = new Orphans(orphanedTableViewModels);
            var renderedOrphans = Formatter.RenderTemplate(templateParameter);

            var orphansContainer = new Container(renderedOrphans, Database.DatabaseName, string.Empty);
            var renderedPage = Formatter.RenderTemplate(orphansContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "orphans.html");

            File.WriteAllText(outputPath, renderedPage);
        }

        public async Task RenderAsync()
        {
            var tablesCollection = await Database.TablesAsync().ConfigureAwait(false);

            var tables = await Task.WhenAll(tablesCollection).ConfigureAwait(false);
            var tableParentKeyCountTasks = tables.Select(t => t.ParentKeysAsync());
            var tableChildKeyCountTasks = tables.Select(t => t.ChildKeysAsync());
            var tableParentKeyCounts = await Task.WhenAll(tableParentKeyCountTasks).ConfigureAwait(false);
            var tableChildKeyCounts = await Task.WhenAll(tableChildKeyCountTasks).ConfigureAwait(false);

            var orphanedTables = tables
                .Where((t, i) => tableParentKeyCounts[i].Count == 0 && tableChildKeyCounts[i].Count == 0)
                .ToList();

            var mapper = new OrphansModelMapper(Connection, Database.Dialect);
            var mappingTasks = orphanedTables.Select(mapper.MapAsync).ToArray();
            var orphanedTableViewModels = await Task.WhenAll(mappingTasks).ConfigureAwait(false);

            var templateParameter = new Orphans(orphanedTableViewModels);
            var renderedOrphans = Formatter.RenderTemplate(templateParameter);

            var orphansContainer = new Container(renderedOrphans, Database.DatabaseName, string.Empty);
            var renderedPage = Formatter.RenderTemplate(orphansContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "orphans.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
