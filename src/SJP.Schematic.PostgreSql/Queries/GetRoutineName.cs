using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetRoutineName
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = $"""

select
    n.nspname as "{nameof(Result.SchemaName)}",
    p.proname as "{nameof(Result.RoutineName)}"
from pg_catalog.pg_proc p
inner join pg_catalog.pg_namespace n on n.oid = p.pronamespace
where n.nspname = @{nameof(Query.SchemaName)} and p.proname = @{nameof(Query.RoutineName)}
    and n.nspname not in ('pg_catalog', 'information_schema')
limit 1
""";
}