using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableName
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal const string Sql = $"""

select
    ns.nspname as "{nameof(Result.SchemaName)}",
    t.relname as "{nameof(Result.TableName)}"
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
where t.relkind in ('r', 'p')
    and not t.relispartition
    and ns.nspname = @{nameof(Query.SchemaName)}
    and t.relname = @{nameof(Query.TableName)}
    and ns.nspname not in ('pg_catalog', 'information_schema')
limit 1
""";
}