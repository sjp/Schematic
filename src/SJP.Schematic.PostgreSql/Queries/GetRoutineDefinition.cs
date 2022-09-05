namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetRoutineDefinition
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }

    internal const string Sql = @$"
select ROUTINE_DEFINITION
from information_schema.routines
where ROUTINE_SCHEMA = @{nameof(Query.SchemaName)} and ROUTINE_NAME = @{nameof(Query.RoutineName)}";
}