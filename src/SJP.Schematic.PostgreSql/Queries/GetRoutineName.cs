namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetRoutineName
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    ROUTINE_SCHEMA as ""{nameof(Result.SchemaName)}"",
    ROUTINE_NAME as ""{nameof(Result.RoutineName)}""
from information_schema.routines
where ROUTINE_SCHEMA = @{nameof(Query.SchemaName)} and ROUTINE_NAME = @{nameof(Query.RoutineName)}
    and ROUTINE_SCHEMA not in ('pg_catalog', 'information_schema')
limit 1";
}