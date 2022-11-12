namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetViewName
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select schemaname as ""{nameof(Result.SchemaName)}"", viewname as ""{nameof(Result.ViewName)}""
from pg_catalog.pg_views
where schemaname = @{nameof(Query.SchemaName)} and viewname = @{nameof(Query.ViewName)}
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1";
}