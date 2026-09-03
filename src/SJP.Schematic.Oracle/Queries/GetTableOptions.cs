using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableOptions
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        /// <summary>Whether the table is a global temporary table, i.e. <c>Y</c> or <c>N</c>.</summary>
        public required string? IsTemporary { get; init; }

        /// <summary>Whether the table is partitioned, i.e. <c>YES</c> or <c>NO</c>.</summary>
        public required string? IsPartitioned { get; init; }

        /// <summary><c>IOT</c> for an index-organized table, otherwise null or an overflow/mapping segment.</summary>
        public required string? IotType { get; init; }

        /// <summary>Whether changes are written to the redo log, i.e. <c>YES</c> or <c>NO</c>. Null for a partitioned table, whose partitions each carry their own setting.</summary>
        public required string? Logging { get; init; }

        /// <summary>Whether the table's data is held outside the database, i.e. <c>Y</c> or <c>N</c>.</summary>
        public required string? IsExternal { get; init; }

        public required string? DefaultCollation { get; init; }

        /// <summary><c>RANGE</c>, <c>LIST</c>, <c>HASH</c>, <c>REFERENCE</c> or <c>SYSTEM</c>. Null when the table is not partitioned.</summary>
        public required string? PartitioningType { get; init; }
    }

    // ALL_TABLES.DEFAULT_COLLATION requires Oracle 12.2 or later.
    internal const string Sql = $"""

select
    t.TEMPORARY as "{nameof(Result.IsTemporary)}",
    t.PARTITIONED as "{nameof(Result.IsPartitioned)}",
    t.IOT_TYPE as "{nameof(Result.IotType)}",
    t.LOGGING as "{nameof(Result.Logging)}",
    t.DEFAULT_COLLATION as "{nameof(Result.DefaultCollation)}",
    pt.PARTITIONING_TYPE as "{nameof(Result.PartitioningType)}",
    case when et.TABLE_NAME is null then 'N' else 'Y' end as "{nameof(Result.IsExternal)}"
from SYS.ALL_TABLES t
left join SYS.ALL_PART_TABLES pt on t.OWNER = pt.OWNER and t.TABLE_NAME = pt.TABLE_NAME
left join SYS.ALL_EXTERNAL_TABLES et on t.OWNER = et.OWNER and t.TABLE_NAME = et.TABLE_NAME
where t.OWNER = :{nameof(Query.SchemaName)} and t.TABLE_NAME = :{nameof(Query.TableName)}
""";
}
