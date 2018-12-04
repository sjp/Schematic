using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class ConstraintsRenderer : ITemplateRenderer
    {
        public ConstraintsRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IDbConnection Connection { get; }

        private IRelationalDatabase Database { get; }

        private IHtmlFormatter Formatter { get; }

        private DirectoryInfo ExportDirectory { get; }

        public void Render()
        {
            var tables = Database.Tables.ToList();

            var primaryKeys = tables.SelectNotNull(t => new { TableName = t.Name, t.PrimaryKey }).ToList();
            var uniqueKeys = tables.SelectMany(t => t.UniqueKeys.Select(uk => new { TableName = t.Name, UniqueKey = uk })).ToList();
            var foreignKeys = tables.SelectMany(t => t.ParentKeys).ToList();
            var checkConstraints = tables.SelectMany(t => t.Checks.Select(ck => new { TableName = t.Name, Check = ck })).ToList();

            var mapper = new ConstraintsModelMapper();

            var primaryKeyViewModels = primaryKeys
                .Select(pk => mapper.MapPrimaryKey(pk.TableName, pk.PrimaryKey))
                .OrderBy(pk => pk.TableName)
                .ToList();
            var uniqueKeyViewModels = uniqueKeys
                .Select(uk => mapper.MapUniqueKey(uk.TableName, uk.UniqueKey))
                .OrderBy(uk => uk.TableName)
                .ToList();
            var foreignKeyViewModels = foreignKeys
                .Select(mapper.MapForeignKey)
                .OrderBy(fk => fk.TableName)
                .ToList();
            var checkConstraintViewModels = checkConstraints
                .Select(ck => mapper.MapCheckConstraint(ck.TableName, ck.Check))
                .OrderBy(ck => ck.TableName)
                .ToList();

            var templateParameter = new Constraints(
                primaryKeyViewModels,
                uniqueKeyViewModels,
                foreignKeyViewModels,
                checkConstraintViewModels
            );
            var renderedConstraints = Formatter.RenderTemplate(templateParameter);

            var constraintsContainer = new Container(renderedConstraints, Database.DatabaseName, string.Empty);
            var renderedPage = Formatter.RenderTemplate(constraintsContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "constraints.html");

            File.WriteAllText(outputPath, renderedPage);
        }

        public async Task RenderAsync()
        {
            var tables = await Database.TablesAsync().ConfigureAwait(false);

            var primaryKeys = tables.SelectNotNull(t => new { TableName = t.Name, t.PrimaryKey }).ToList();
            var uniqueKeys = tables.SelectMany(t => t.UniqueKeys.Select(uk => new { TableName = t.Name, UniqueKey = uk })).ToList();
            var foreignKeys = tables.SelectMany(t => t.ParentKeys).ToList();
            var checkConstraints = tables.SelectMany(t => t.Checks.Select(ck => new { TableName = t.Name, Check = ck })).ToList();

            var mapper = new ConstraintsModelMapper();

            var primaryKeyViewModels = primaryKeys
                .Select(pk => mapper.MapPrimaryKey(pk.TableName, pk.PrimaryKey))
                .OrderBy(pk => pk.TableName)
                .ToList();
            var uniqueKeyViewModels = uniqueKeys
                .Select(uk => mapper.MapUniqueKey(uk.TableName, uk.UniqueKey))
                .OrderBy(uk => uk.TableName)
                .ToList();
            var foreignKeyViewModels = foreignKeys
                .Select(mapper.MapForeignKey)
                .OrderBy(fk => fk.TableName)
                .ToList();
            var checkConstraintViewModels = checkConstraints
                .Select(ck => mapper.MapCheckConstraint(ck.TableName, ck.Check))
                .OrderBy(ck => ck.TableName)
                .ToList();

            var templateParameter = new Constraints(
                primaryKeyViewModels,
                uniqueKeyViewModels,
                foreignKeyViewModels,
                checkConstraintViewModels
            );
            var renderedConstraints = Formatter.RenderTemplate(templateParameter);

            var constraintsContainer = new Container(renderedConstraints, Database.DatabaseName, string.Empty);
            var renderedPage = Formatter.RenderTemplate(constraintsContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "constraints.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
