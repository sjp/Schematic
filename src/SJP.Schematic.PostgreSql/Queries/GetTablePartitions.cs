using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTablePartitions
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

    // A partitioned table's children in pg_inherits are exactly its partitions.
    internal const string Sql = $"""

select
    cns.nspname as "{nameof(Result.SchemaName)}",
    c.relname as "{nameof(Result.TableName)}"
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
inner join pg_catalog.pg_inherits inh on inh.inhparent = t.oid
inner join pg_catalog.pg_class c on c.oid = inh.inhrelid
inner join pg_catalog.pg_namespace cns on cns.oid = c.relnamespace
where t.relkind = 'p'
    and ns.nspname = @{nameof(Query.SchemaName)}
    and t.relname = @{nameof(Query.TableName)}
order by cns.nspname, c.relname
""";
}
