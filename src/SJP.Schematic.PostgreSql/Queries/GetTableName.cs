namespace SJP.Schematic.PostgreSql.Queries;

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
select schemaname as ""{ nameof(Result.SchemaName) }"", tablename as ""{ nameof(Result.TableName) }""
from pg_catalog.pg_tables
where schemaname = @{ nameof(Query.SchemaName) } and tablename = @{ nameof(Query.TableName) }
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1";
}