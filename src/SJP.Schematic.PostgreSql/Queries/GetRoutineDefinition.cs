namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetRoutineDefinition
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = @$"
select ROUTINE_DEFINITION
from information_schema.routines
where ROUTINE_SCHEMA = @{nameof(Query.SchemaName)} and ROUTINE_NAME = @{nameof(Query.RoutineName)}";
}