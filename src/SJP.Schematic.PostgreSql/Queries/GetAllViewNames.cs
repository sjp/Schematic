namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetAllViewNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select schemaname as ""{nameof(Result.SchemaName)}"", viewname as ""{nameof(Result.ViewName)}""
from pg_catalog.pg_views
where schemaname not in ('pg_catalog', 'information_schema')
order by schemaname, viewname";
}