using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class ConstraintsRenderer : ITemplateRenderer
    {
        public ConstraintsRenderer(
            IIdentifierDefaults identifierDefaults,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IRelationalDatabaseTable> tables,
            DirectoryInfo exportDirectory
        )
        {
            if (tables == null || tables.AnyNull())
                throw new ArgumentNullException(nameof(tables));

            Tables = tables;
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IIdentifierDefaults IdentifierDefaults { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            var primaryKeys = Tables.SelectMany(t => t.PrimaryKey.Select(pk => new { TableName = t.Name, PrimaryKey = pk })).ToList();
            var uniqueKeys = Tables.SelectMany(t => t.UniqueKeys.Select(uk => new { TableName = t.Name, UniqueKey = uk })).ToList();
            var foreignKeys = Tables.SelectMany(t => t.ParentKeys).ToList();
            var checkConstraints = Tables.SelectMany(t => t.Checks.Select(ck => new { TableName = t.Name, Check = ck })).ToList();

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

            var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? IdentifierDefaults.Database + " Database"
                : "Database";
            var pageTitle = "Constraints · " + databaseName;
            var constraintsContainer = new Container(renderedConstraints, pageTitle, string.Empty);
            var renderedPage = Formatter.RenderTemplate(constraintsContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "constraints.html");

            using (var writer = File.CreateText(outputPath))
            {
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}
