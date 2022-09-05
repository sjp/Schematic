namespace SJP.Schematic.MySql.Queries;

internal static class GetAllRoutineNames
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    ROUTINE_SCHEMA as `{nameof(Result.SchemaName)}`,
    ROUTINE_NAME as `{nameof(Result.RoutineName)}`
from information_schema.routines
where ROUTINE_SCHEMA = @{nameof(Query.SchemaName)}
order by ROUTINE_SCHEMA, ROUTINE_NAME";
}