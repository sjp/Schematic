namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetAllTableStatistics
{
    internal sealed record Result : ITableStatisticsRow
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }

        public required long RowCount { get; init; }

        public required long? DataSizeBytes { get; init; }

        public required long? IndexSizeBytes { get; init; }
    }

    // reltuples is the planner's estimate, last refreshed by ANALYZE or VACUUM. It is negative
    // when the table has never been analysed, which is reported as an absent count rather than as
    // an empty table. A partitioned table stores nothing itself, so its sizes are zero.
    internal const string Sql = $"""

select
    ns.nspname as "{nameof(Result.SchemaName)}",
    t.relname as "{nameof(Result.TableName)}",
    cast(t.reltuples as bigint) as "{nameof(Result.RowCount)}",
    pg_catalog.pg_table_size(t.oid) as "{nameof(Result.DataSizeBytes)}",
    pg_catalog.pg_indexes_size(t.oid) as "{nameof(Result.IndexSizeBytes)}"
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
where t.relkind in ('r', 'p')
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and not pg_catalog.pg_is_other_temp_schema(ns.oid)
order by ns.nspname, t.relname
""";
}
