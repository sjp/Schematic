using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableStatistics
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result : ITableStatisticsRow
    {
        public required decimal? RowCount { get; init; }

        public required decimal? DataSizeBytes { get; init; }

        public required decimal? IndexSizeBytes { get; init; }
    }

    // See GetAllTableStatistics for where these values come from.
    internal const string Sql = $"""

select
    t.NUM_ROWS as "{nameof(Result.RowCount)}",
    t.BLOCKS * ts.BLOCK_SIZE as "{nameof(Result.DataSizeBytes)}",
    (
        select sum(i.LEAF_BLOCKS)
        from SYS.ALL_INDEXES i
        where i.TABLE_OWNER = t.OWNER and i.TABLE_NAME = t.TABLE_NAME
    ) * ts.BLOCK_SIZE as "{nameof(Result.IndexSizeBytes)}"
from SYS.ALL_TABLES t
left join SYS.ALL_TABLESPACES ts on t.TABLESPACE_NAME = ts.TABLESPACE_NAME
where t.OWNER = :{nameof(Query.SchemaName)} and t.TABLE_NAME = :{nameof(Query.TableName)}
""";
}
