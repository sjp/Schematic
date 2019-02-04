using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Dot;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class RelationshipsModelMapper
    {
        public RelationshipsModelMapper(IDbConnection connection, IDatabaseDialect dialect, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        private IDbConnection Connection { get; }

        private IDatabaseDialect Dialect { get; }

        private IIdentifierDefaults IdentifierDefaults { get; }

        public Task<Relationships> MapAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return MapAsyncCore(tables, cancellationToken);
        }

        private async Task<Relationships> MapAsyncCore(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken)
        {
            var dotFormatter = new DatabaseDotFormatter(Connection, Dialect, IdentifierDefaults);

            var rootPath = string.Empty;
            var compactOptions = new DotRenderOptions
            {
                IsReducedColumnSet = true,
                RootPath = rootPath
            };
            var compactDot = await dotFormatter.RenderTablesAsync(tables, compactOptions, cancellationToken).ConfigureAwait(false);

            var largeOptions = new DotRenderOptions
            {
                IsReducedColumnSet = false,
                RootPath = rootPath
            };
            var largeDot = await dotFormatter.RenderTablesAsync(tables, largeOptions, cancellationToken).ConfigureAwait(false);

            var diagrams = new[]
            {
                new Relationships.Diagram("Compact", compactDot, true),
                new Relationships.Diagram("Large", largeDot, false)
            };

            return new Relationships(diagrams);
        }
    }
}
