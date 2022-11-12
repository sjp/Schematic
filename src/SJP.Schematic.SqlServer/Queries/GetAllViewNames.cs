namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllViewNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select schema_name(schema_id) as [{nameof(Result.SchemaName)}], name as [{nameof(Result.ViewName)}]
from sys.views
where is_ms_shipped = 0
order by schema_name(schema_id), name";
}