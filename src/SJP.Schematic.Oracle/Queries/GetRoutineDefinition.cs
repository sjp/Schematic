namespace SJP.Schematic.Oracle.Queries;

internal static class GetRoutineDefinition
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }

    internal const string Sql = @$"
select TEXT
from SYS.ALL_SOURCE
where OWNER = :{nameof(Query.SchemaName)} and NAME = :{nameof(Query.RoutineName)}
    AND TYPE IN ('FUNCTION', 'PROCEDURE')
order by LINE";
}