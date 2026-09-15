using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetMaterializedViewDefinition
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal sealed record Result
    {
        /// <summary>
        /// The query text of the materialized view.
        /// </summary>
        public required string? Definition { get; init; }

        /// <summary>
        /// Whether the materialized view holds data. One created <c>WITH NO DATA</c> holds no rows and
        /// cannot be queried until it has been refreshed.
        /// </summary>
        public required bool IsPopulated { get; init; }
    }

    internal const string Sql = $"""

select
    definition as "{nameof(Result.Definition)}",
    ispopulated as "{nameof(Result.IsPopulated)}"
from pg_catalog.pg_matviews
where schemaname = @{nameof(Query.SchemaName)} and matviewname = @{nameof(Query.ViewName)}
limit 1
""";
}
