using System;
using System.Data;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Dot;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal class RelationshipsModelMapper :
        IDatabaseModelMapper<IRelationalDatabase, Relationships>
    {
        public RelationshipsModelMapper(IDbConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        protected IDbConnection Connection { get; }

        public Relationships Map(IRelationalDatabase dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var dotFormatter = new DatabaseDotFormatter(Connection, dbObject);

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
                new Relationships.Diagram("Compact", compactDot) { IsActive = true },
                new Relationships.Diagram("Large", largeDot)
            };

            return new Relationships { Diagrams = diagrams };
        }

        public async Task<Relationships> MapAsync(IRelationalDatabase dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var dotFormatter = new DatabaseDotFormatter(Connection, dbObject);

            var rootPath = string.Empty;
            var compactOptions = new DotRenderOptions
            {
                IsReducedColumnSet = true,
                RootPath = rootPath
            };
            var compactDot = await dotFormatter.RenderDatabaseAsync(compactOptions).ConfigureAwait(false);

            var largeOptions = new DotRenderOptions
            {
                IsReducedColumnSet = false,
                RootPath = rootPath
            };
            var largeDot = await dotFormatter.RenderDatabaseAsync(largeOptions).ConfigureAwait(false);

            var diagrams = new[]
            {
                new Relationships.Diagram("Compact", compactDot) { IsActive = true },
                new Relationships.Diagram("Large", largeDot)
            };

            return new Relationships { Diagrams = diagrams };
        }
    }
}
