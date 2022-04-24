namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetAllTableNames
{
    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    schemaname as ""{ nameof(Result.SchemaName) }"",
    tablename as ""{ nameof(Result.TableName) }""
from pg_catalog.pg_tables
where schemaname not in ('pg_catalog', 'information_schema')
order by schemaname, tablename";}