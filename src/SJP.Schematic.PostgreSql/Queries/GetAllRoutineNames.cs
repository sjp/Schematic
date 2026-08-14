namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetAllRoutineNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = $"""

select distinct
    n.nspname as "{nameof(Result.SchemaName)}",
    p.proname as "{nameof(Result.RoutineName)}"
from pg_catalog.pg_proc p
inner join pg_catalog.pg_namespace n on n.oid = p.pronamespace
where n.nspname not in ('pg_catalog', 'information_schema')
    and not pg_catalog.pg_is_other_temp_schema(n.oid)
order by n.nspname, p.proname
""";
}
