namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetAllMaterializedViewNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = $"""

select schemaname as "{nameof(Result.SchemaName)}", matviewname as "{nameof(Result.ViewName)}"
from pg_catalog.pg_matviews
where schemaname not in ('pg_catalog', 'information_schema')
order by schemaname, matviewname

""";
}