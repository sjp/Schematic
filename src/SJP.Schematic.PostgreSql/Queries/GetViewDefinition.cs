using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetViewDefinition
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    // pg_views.definition is pg_get_viewdef(oid) with no privilege check, whereas
    // information_schema.views.view_definition wraps the same call in a pg_has_role() guard and
    // returns null when the caller does not own the view. The text produced is identical.
    internal const string Sql = $"""

select definition
from pg_catalog.pg_views
where schemaname = @{nameof(Query.SchemaName)} and viewname = @{nameof(Query.ViewName)}
limit 1
""";
}