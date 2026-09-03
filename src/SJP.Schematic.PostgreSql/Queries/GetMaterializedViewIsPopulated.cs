using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetMaterializedViewIsPopulated
{
    internal sealed record Query : ISqlQuery<bool>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    // A materialized view created WITH NO DATA holds no rows and cannot be queried until it has been
    // refreshed; pg_class.relispopulated records which of the two states it is in.
    internal const string Sql = $"""

select t.relispopulated
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
where t.relkind = 'm' and t.relname = @{nameof(Query.ViewName)} and ns.nspname = @{nameof(Query.SchemaName)}
""";
}
