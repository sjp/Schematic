namespace SJP.Schematic.MySql.Queries;

internal static class GetAllTableNames
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    table_schema as `{nameof(Result.SchemaName)}`,
    table_name as `{nameof(Result.TableName)}`
from information_schema.tables
where table_schema = @{nameof(Query.SchemaName)}";
}