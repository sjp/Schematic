namespace SJP.Schematic.Oracle.Queries;

internal static class GetRoutineDefinition
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = @$"
select TEXT
from SYS.ALL_SOURCE
where OWNER = :{nameof(Query.SchemaName)} and NAME = :{nameof(Query.RoutineName)}
    AND TYPE IN ('FUNCTION', 'PROCEDURE')
order by LINE";
}