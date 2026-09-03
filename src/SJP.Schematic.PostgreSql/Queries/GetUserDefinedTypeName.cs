using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetUserDefinedTypeName
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TypeName { get; init; }
    }

    internal sealed record Result
    {
        public required string? SchemaName { get; init; }

        public required string? TypeName { get; init; }
    }

    internal const string Sql = $"""

select
    n.nspname as "{nameof(Result.SchemaName)}",
    t.typname as "{nameof(Result.TypeName)}"
from pg_catalog.pg_type t
inner join pg_catalog.pg_namespace n on n.oid = t.typnamespace
left join pg_catalog.pg_class cls on cls.oid = t.typrelid
where n.nspname = @{nameof(Query.SchemaName)} and t.typname = @{nameof(Query.TypeName)}
    and n.nspname not in ('pg_catalog', 'information_schema', 'pg_toast')
    and t.typtype in ('d', 'e', 'c', 'r')
    and (t.typtype <> 'c' or cls.relkind = 'c')
limit 1
""";
}
