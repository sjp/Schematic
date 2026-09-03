namespace SJP.Schematic.Oracle.Queries;

internal static class GetAllTableStatistics
{
    internal sealed record Result : ITableStatisticsRow
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }

        public required decimal? RowCount { get; init; }

        public required decimal? DataSizeBytes { get; init; }

        public required decimal? IndexSizeBytes { get; init; }
    }

    // Every value comes from the optimizer statistics that DBMS_STATS gathers, so all of them are
    // null until a table has been analysed. Sizes are derived from the blocks the statistics
    // record, which needs the block size of the tablespace holding the table; a tablespace the
    // user cannot see leaves the sizes absent. The table filters match GetAllTableNames so that
    // the same set of tables is described.
    internal const string Sql = $"""

select
    t.OWNER as "{nameof(Result.SchemaName)}",
    t.TABLE_NAME as "{nameof(Result.TableName)}",
    t.NUM_ROWS as "{nameof(Result.RowCount)}",
    t.BLOCKS * ts.BLOCK_SIZE as "{nameof(Result.DataSizeBytes)}",
    (
        select sum(i.LEAF_BLOCKS)
        from SYS.ALL_INDEXES i
        where i.TABLE_OWNER = t.OWNER and i.TABLE_NAME = t.TABLE_NAME
    ) * ts.BLOCK_SIZE as "{nameof(Result.IndexSizeBytes)}"
from SYS.ALL_TABLES t
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
left join SYS.ALL_NESTED_TABLES nt on t.OWNER = nt.OWNER and t.TABLE_NAME = nt.TABLE_NAME
left join SYS.ALL_TABLESPACES ts on t.TABLESPACE_NAME = ts.TABLESPACE_NAME
where
    o.OBJECT_TYPE = 'TABLE'
    and o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and o.SUBOBJECT_NAME is null
    and mv.MVIEW_NAME is null
    and nt.TABLE_NAME is null
order by t.OWNER, t.TABLE_NAME
""";
}
