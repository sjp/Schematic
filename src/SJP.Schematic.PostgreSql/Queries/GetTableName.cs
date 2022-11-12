namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableName
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal const string Sql = @$"
select schemaname as ""{nameof(Result.SchemaName)}"", tablename as ""{nameof(Result.TableName)}""
from pg_catalog.pg_tables
where schemaname = @{nameof(Query.SchemaName)} and tablename = @{nameof(Query.TableName)}
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1";
}