namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllRoutineNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = @$"
select
    s.name as [{nameof(Result.SchemaName)}],
    o.name as [{nameof(Result.RoutineName)}]
from sys.objects o
inner join sys.schemas s on o.schema_id = s.schema_id
where o.type in ('P', 'FN', 'IF', 'TF') and o.is_ms_shipped = 0
order by s.name, o.name";
}