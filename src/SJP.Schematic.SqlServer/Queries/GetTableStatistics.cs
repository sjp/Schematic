using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableStatistics
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result : ITableStatisticsRow
    {
        public required long? RowCount { get; init; }

        public required long? DataSizeBytes { get; init; }

        public required long? IndexSizeBytes { get; init; }
    }

    // See GetAllTableStatistics for how the partition rows are aggregated.
    internal const string Sql = @$"
select
    sum(case when ps.index_id in (0, 1) then ps.row_count else 0 end) as [{nameof(Result.RowCount)}],
    sum(ps.in_row_data_page_count + ps.lob_used_page_count + ps.row_overflow_used_page_count) * 8192 as [{nameof(Result.DataSizeBytes)}],
    (sum(ps.used_page_count) - sum(ps.in_row_data_page_count + ps.lob_used_page_count + ps.row_overflow_used_page_count)) * 8192 as [{nameof(Result.IndexSizeBytes)}]
from sys.tables t
inner join sys.dm_db_partition_stats ps on ps.object_id = t.object_id
where t.schema_id = schema_id(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0
group by t.object_id";
}
