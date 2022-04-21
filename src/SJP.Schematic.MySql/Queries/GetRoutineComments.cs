namespace SJP.Schematic.MySql.Queries;

internal static class GetRoutineComments
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }

    internal const string Sql = @$"
select ROUTINE_COMMENT
from information_schema.routines
where ROUTINE_SCHEMA = @{ nameof(Query.SchemaName) } and ROUTINE_NAME = @{ nameof(Query.RoutineName) }";
}