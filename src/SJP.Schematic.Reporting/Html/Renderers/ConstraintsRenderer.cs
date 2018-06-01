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
    internal class ConstraintsRenderer : ITemplateRenderer
    {
        public ConstraintsRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
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
            var tables = Database.Tables.ToList();

            var primaryKeys = tables.SelectNotNull(t => t.PrimaryKey).ToList();
            var uniqueKeys = tables.SelectMany(t => t.UniqueKeys).ToList();
            var foreignKeys = tables.SelectMany(t => t.ParentKeys).ToList();
            var checkConstraints = tables.SelectMany(t => t.Checks).ToList();

            var mapper = new ConstraintsModelMapper();
            var pkMapper = mapper as IDatabaseModelMapper<IDatabaseKey, Constraints.PrimaryKeyConstraint>;
            var ukMapper = mapper as IDatabaseModelMapper<IDatabaseKey, Constraints.UniqueKey>;

            var primaryKeyViewModels = primaryKeys.Select(pkMapper.Map).OrderBy(pk => pk.TableName).ToList();
            var uniqueKeyViewModels = uniqueKeys.Select(ukMapper.Map).OrderBy(uk => uk.TableName).ToList();
            var foreignKeyViewModels = foreignKeys.Select(mapper.Map).OrderBy(fk => fk.TableName).ToList();
            var checkConstraintViewModels = checkConstraints.Select(mapper.Map).OrderBy(ck => ck.TableName).ToList();

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
            var tableCollection = await Database.TablesAsync().ConfigureAwait(false);
            var tables = await tableCollection.ToList().ConfigureAwait(false);

            var primaryKeys = await tables.SelectNotNullAsync(t => t.PrimaryKeyAsync()).ConfigureAwait(false);
            var uniqueKeys = await tables.SelectManyAsync(t => t.UniqueKeysAsync()).ConfigureAwait(false);
            var foreignKeys = await tables.SelectManyAsync(t => t.ParentKeysAsync()).ConfigureAwait(false);
            var checkConstraints = await tables.SelectManyAsync(t => t.ChecksAsync()).ConfigureAwait(false);

            var mapper = new ConstraintsModelMapper();
            var pkMapper = mapper as IDatabaseModelMapper<IDatabaseKey, Constraints.PrimaryKeyConstraint>;
            var ukMapper = mapper as IDatabaseModelMapper<IDatabaseKey, Constraints.UniqueKey>;

            var primaryKeyViewModels = primaryKeys.Select(pkMapper.Map).OrderBy(pk => pk.TableName).ToList();
            var uniqueKeyViewModels = uniqueKeys.Select(ukMapper.Map).OrderBy(uk => uk.TableName).ToList();
            var foreignKeyViewModels = foreignKeys.Select(mapper.Map).OrderBy(fk => fk.TableName).ToList();
            var checkConstraintViewModels = checkConstraints.Select(mapper.Map).OrderBy(ck => ck.TableName).ToList();

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
