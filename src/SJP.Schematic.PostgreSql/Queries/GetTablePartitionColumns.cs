using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTablePartitionColumns
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    // partattrs is an int2vector, which subscripts from zero. An entry of zero stands for an
    // expression rather than a column, and is skipped by the join to pg_attribute.
    internal const string Sql = $"""

select a.attname
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
inner join pg_catalog.pg_partitioned_table pt on pt.partrelid = t.oid
cross join lateral generate_series(0, pt.partnatts - 1) as k(idx)
inner join pg_catalog.pg_attribute a on a.attrelid = t.oid and a.attnum = pt.partattrs[k.idx]
where ns.nspname = @{nameof(Query.SchemaName)}
    and t.relname = @{nameof(Query.TableName)}
order by k.idx
""";
}
