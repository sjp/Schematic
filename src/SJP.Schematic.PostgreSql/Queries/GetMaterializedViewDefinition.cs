namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetMaterializedViewDefinition
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal const string Sql = @$"
select definition
from pg_catalog.pg_matviews
where schemaname = @{nameof(Query.SchemaName)} and matviewname = @{nameof(Query.ViewName)}";
}