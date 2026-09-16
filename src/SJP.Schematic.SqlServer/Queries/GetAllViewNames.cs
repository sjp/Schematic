namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllViewNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select s.name as [{nameof(Result.SchemaName)}], v.name as [{nameof(Result.ViewName)}]
from sys.views v
inner join sys.schemas s on v.schema_id = s.schema_id
where v.is_ms_shipped = 0
order by s.name, v.name";
}