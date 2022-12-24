using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetMaterializedViewName
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
select schemaname as ""{nameof(Result.SchemaName)}"", matviewname as ""{nameof(Result.ViewName)}""
from pg_catalog.pg_matviews
where schemaname = @{nameof(Result.SchemaName)} and matviewname = @{nameof(Result.ViewName)}
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1";
}