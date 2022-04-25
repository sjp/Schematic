namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableName
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal const string Sql = @$"
select top 1 schema_name(schema_id) as [{ nameof(Result.SchemaName) }], name as [{ nameof(Result.TableName) }]
from sys.tables
where schema_id = schema_id(@{ nameof(Query.SchemaName) }) and name = @{ nameof(Query.TableName) } and is_ms_shipped = 0";
}