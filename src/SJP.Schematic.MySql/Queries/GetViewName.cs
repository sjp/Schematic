using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetViewName
{
    internal sealed record Query : ISqlQuery<Result>
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
select
    table_schema as `{nameof(Result.SchemaName)}`,
    table_name as `{nameof(Result.ViewName)}`
from information_schema.views
where table_schema = @{nameof(Query.SchemaName)} and table_name = @{nameof(Query.ViewName)}
limit 1";
}