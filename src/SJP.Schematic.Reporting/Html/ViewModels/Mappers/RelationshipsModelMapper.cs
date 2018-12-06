using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Dot;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class RelationshipsModelMapper
    {
        public RelationshipsModelMapper(IDbConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        private IDbConnection Connection { get; }

        public Relationships Map(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            var dotFormatter = new DatabaseDotFormatter(Connection, database);

            var rootPath = string.Empty;
            var compactOptions = new DotRenderOptions
            {
                IsReducedColumnSet = true,
                RootPath = rootPath
            };
            var compactDot = dotFormatter.RenderDatabase(compactOptions);

            var largeOptions = new DotRenderOptions
            {
                IsReducedColumnSet = false,
                RootPath = rootPath
            };
            var largeDot = dotFormatter.RenderDatabase(largeOptions);

            var diagrams = new[]
            {
                new Relationships.Diagram("Compact", compactDot, true),
                new Relationships.Diagram("Large", largeDot, false)
            };

            return new Relationships(diagrams);
        }

        public Task<Relationships> MapAsync(IRelationalDatabase database, CancellationToken cancellationToken)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return MapAsyncCore(database, cancellationToken);
        }

        private async Task<Relationships> MapAsyncCore(IRelationalDatabase database, CancellationToken cancellationToken)
        {
            var dotFormatter = new DatabaseDotFormatter(Connection, database);

            var rootPath = string.Empty;
            var compactOptions = new DotRenderOptions
            {
                IsReducedColumnSet = true,
                RootPath = rootPath
            };
            var compactDot = await dotFormatter.RenderDatabaseAsync(compactOptions, cancellationToken).ConfigureAwait(false);

            var largeOptions = new DotRenderOptions
            {
                IsReducedColumnSet = false,
                RootPath = rootPath
            };
            var largeDot = await dotFormatter.RenderDatabaseAsync(largeOptions, cancellationToken).ConfigureAwait(false);

            var diagrams = new[]
            {
                new Relationships.Diagram("Compact", compactDot, true),
                new Relationships.Diagram("Large", largeDot, false)
            };

            return new Relationships(diagrams);
        }
    }
}
