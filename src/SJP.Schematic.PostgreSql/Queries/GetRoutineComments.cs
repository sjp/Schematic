using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetRoutineComments
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal sealed record Result
    {
        public required string? Comment { get; init; }
    }

    internal const string Sql = $"""

select
    (pg_catalog.array_agg(d.description order by (d.description is null), p.oid))[1] as "{nameof(Result.Comment)}"
from pg_catalog.pg_proc p
inner join pg_catalog.pg_namespace n on n.oid = p.pronamespace
left join pg_catalog.pg_description d
    on d.objoid = p.oid
    and d.classoid = 'pg_catalog.pg_proc'::regclass
    and d.objsubid = 0
where n.nspname = @{nameof(Query.SchemaName)} and p.proname = @{nameof(Query.RoutineName)}
    and n.nspname not in ('pg_catalog', 'information_schema')
limit 1

""";
}