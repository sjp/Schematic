namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllTableNames
{
    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal const string Sql = @$"
select schema_name(schema_id) as [{nameof(Result.SchemaName)}], name as [{nameof(Result.TableName)}]
from sys.tables
where is_ms_shipped = 0
order by schema_name(schema_id), name";
}