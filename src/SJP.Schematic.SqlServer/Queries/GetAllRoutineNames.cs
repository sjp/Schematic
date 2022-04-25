namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllRoutineNames
{
    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    schema_name(schema_id) as [{ nameof(Result.SchemaName) }],
    name as [{ nameof(Result.RoutineName) }]
from sys.objects
where type in ('P', 'FN', 'IF', 'TF') and is_ms_shipped = 0
order by schema_name(schema_id), name";
}