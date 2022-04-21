namespace SJP.Schematic.MySql.Queries;

internal sealed record GetAllViewNames
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    TABLE_SCHEMA as `{ nameof(Result.SchemaName) }`,
    TABLE_NAME as `{ nameof(Result.ViewName) }`
from information_schema.views
where TABLE_SCHEMA = @{ nameof(Query.SchemaName) } order by TABLE_NAME";
}