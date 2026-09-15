using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetViewDefinition
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal sealed record Result
    {
        /// <summary>
        /// The query text of the view.
        /// </summary>
        public required string? Definition { get; init; }

        /// <summary>
        /// The view's check option, one of <c>NONE</c>, <c>LOCAL</c> or <c>CASCADED</c>.
        /// </summary>
        public required string CheckOption { get; init; }

        /// <summary>
        /// Whether rows can be both updated and deleted through the view.
        /// </summary>
        public required bool IsUpdatable { get; init; }
    }

    // These are the expressions information_schema.views uses for view_definition, check_option and
    // is_updatable (20 is the UPDATE and DELETE bits of pg_relation_is_updatable), read from pg_class
    // directly. information_schema.views leaves out every view the current role neither owns nor holds
    // a privilege on, and returns a null definition for views the role does not own; pg_class does
    // neither, so such views report their options the same way they report their definition and columns.
    internal const string Sql = $"""

select
    pg_catalog.pg_get_viewdef(c.oid) as "{nameof(Result.Definition)}",
    case
        when 'check_option=cascaded' = any(c.reloptions) then 'CASCADED'
        when 'check_option=local' = any(c.reloptions) then 'LOCAL'
        else 'NONE'
    end as "{nameof(Result.CheckOption)}",
    (pg_catalog.pg_relation_is_updatable(c.oid::regclass, false) & 20) = 20 as "{nameof(Result.IsUpdatable)}"
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace ns on ns.oid = c.relnamespace
where c.relkind = 'v' and ns.nspname = @{nameof(Query.SchemaName)} and c.relname = @{nameof(Query.ViewName)}
""";
}
