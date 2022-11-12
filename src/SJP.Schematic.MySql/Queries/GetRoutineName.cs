namespace SJP.Schematic.MySql.Queries;

internal static class GetRoutineName
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = @$"
select
    ROUTINE_SCHEMA as `{nameof(Result.SchemaName)}`,
    ROUTINE_NAME as `{nameof(Result.RoutineName)}`
from information_schema.routines
where ROUTINE_SCHEMA = @{nameof(Query.SchemaName)} and ROUTINE_NAME = @{nameof(Query.RoutineName)}
limit 1";
}