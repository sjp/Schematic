namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetAllMaterializedViewNames
{
    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal const string Sql = @$"
select schemaname as ""{nameof(Result.SchemaName)}"", matviewname as ""{nameof(Result.ViewName)}""
from pg_catalog.pg_matviews
where schemaname not in ('pg_catalog', 'information_schema')
order by schemaname, matviewname
";
}