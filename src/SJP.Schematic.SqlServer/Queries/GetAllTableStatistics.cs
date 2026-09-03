namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllTableStatistics
{
    internal sealed record Result : ITableStatisticsRow
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }

        public required long? RowCount { get; init; }

        public required long? DataSizeBytes { get; init; }

        public required long? IndexSizeBytes { get; init; }
    }

    // Rows are counted once per partition of the heap (index_id 0) or clustered index (index_id 1),
    // a table having exactly one of the two. Pages holding rows are data; every other page the
    // table reserves belongs to one of its non-clustered indexes. A memory-optimized table has no
    // partitions here at all, so it is simply absent from the result.
    internal const string Sql = @$"
select
    schema_name(t.schema_id) as [{nameof(Result.SchemaName)}],
    t.name as [{nameof(Result.TableName)}],
    sum(case when ps.index_id in (0, 1) then ps.row_count else 0 end) as [{nameof(Result.RowCount)}],
    sum(ps.in_row_data_page_count + ps.lob_used_page_count + ps.row_overflow_used_page_count) * 8192 as [{nameof(Result.DataSizeBytes)}],
    (sum(ps.used_page_count) - sum(ps.in_row_data_page_count + ps.lob_used_page_count + ps.row_overflow_used_page_count)) * 8192 as [{nameof(Result.IndexSizeBytes)}]
from sys.tables t
inner join sys.dm_db_partition_stats ps on ps.object_id = t.object_id
where t.is_ms_shipped = 0
group by t.schema_id, t.name
order by schema_name(t.schema_id), t.name";
}
