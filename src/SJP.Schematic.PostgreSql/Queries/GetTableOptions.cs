using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableOptions
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        /// <summary>'p' = permanent, 'u' = unlogged, 't' = temporary.</summary>
        public required string Persistence { get; init; }

        /// <summary>'r' = an ordinary table, 'p' = a partitioned table.</summary>
        public required string RelKind { get; init; }

        public required bool IsPartition { get; init; }

        /// <summary>'r' = range, 'l' = list, 'h' = hash. Null when the table is not partitioned.</summary>
        public required string? PartitionStrategy { get; init; }
    }

    internal const string Sql = $"""

select
    t.relpersistence as "{nameof(Result.Persistence)}",
    t.relkind as "{nameof(Result.RelKind)}",
    t.relispartition as "{nameof(Result.IsPartition)}",
    pt.partstrat as "{nameof(Result.PartitionStrategy)}"
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
left join pg_catalog.pg_partitioned_table pt on pt.partrelid = t.oid
where t.relkind in ('r', 'p')
    and ns.nspname = @{nameof(Query.SchemaName)}
    and t.relname = @{nameof(Query.TableName)}
""";
}
