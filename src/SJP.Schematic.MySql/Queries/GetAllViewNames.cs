namespace SJP.Schematic.MySql.Queries;

internal sealed record GetAllViewNames
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select
    TABLE_SCHEMA as `{nameof(Result.SchemaName)}`,
    TABLE_NAME as `{nameof(Result.ViewName)}`
from information_schema.views
where TABLE_SCHEMA = @{nameof(Query.SchemaName)} order by TABLE_NAME";
}