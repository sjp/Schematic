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
    schema_name(schema_id) as [{nameof(Result.SchemaName)}],
    name as [{nameof(Result.RoutineName)}]
from sys.objects
where type in ('P', 'FN', 'IF', 'TF') and is_ms_shipped = 0
order by schema_name(schema_id), name";
}