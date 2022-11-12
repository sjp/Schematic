namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetRoutineComments
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal sealed record Result
    {
        public required string? Comment { get; init; }
    }

    internal const string Sql = @$"
select
    d.description as ""{nameof(Result.Comment)}""
from pg_catalog.pg_proc p
inner join pg_namespace n on n.oid = p.pronamespace
left join pg_catalog.pg_description d on p.oid = d.objoid
where n.nspname = @{nameof(Query.SchemaName)} and p.proname = @{nameof(Query.RoutineName)}
    and n.nspname not in ('pg_catalog', 'information_schema')
";
}