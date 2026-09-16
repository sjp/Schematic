namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllTableNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal const string Sql = @$"
select s.name as [{nameof(Result.SchemaName)}], t.name as [{nameof(Result.TableName)}]
from sys.tables t
inner join sys.schemas s on t.schema_id = s.schema_id
where t.is_ms_shipped = 0
order by s.name, t.name";
}