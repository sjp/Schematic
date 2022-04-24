namespace SJP.Schematic.MySql.Queries;

internal static class GetViewName
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    table_schema as `{ nameof(Result.SchemaName) }`,
    table_name as `{ nameof(Result.ViewName) }`
from information_schema.views
where table_schema = @{ nameof(Query.SchemaName) } and table_name = @{ nameof(Query.ViewName) }
limit 1";
}