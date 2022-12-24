using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetMaterializedViewDefinition
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select definition
from pg_catalog.pg_matviews
where schemaname = @{nameof(Query.SchemaName)} and matviewname = @{nameof(Query.ViewName)}";
}