using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableDetails
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    /// <summary>
    /// A table's options, together with one of its triggers. The options repeat on every row; a table
    /// without triggers has a single row whose trigger columns are all <see langword="null" />.
    /// </summary>
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

        public required string? TriggerSchema { get; init; }

        /// <summary>The trigger's name, or <see langword="null" /> on the single row of a table without triggers.</summary>
        public required string? TriggerName { get; init; }

        public required string? TriggerType { get; init; }

        public required string? TriggerEvent { get; init; }

        public required string? TriggerCondition { get; init; }

        /// <summary>The comma-separated <c>UPDATE OF</c> column list, or <see langword="null" /> when there is none.</summary>
        public required string? TriggerUpdateColumns { get; init; }

        public required string? TriggerDefinition { get; init; }

        public required string? TriggerEnabledStatus { get; init; }
    }

    // ALL_TABLES.DEFAULT_COLLATION requires Oracle 12.2 or later.
    //
    // The triggers are outer joined rather than appended with union all, because TRIGGER_BODY is a LONG
    // and Oracle does not allow a LONG column in a set operation. ALL_TRIGGERS does not record the
    // UPDATE OF column list; ALL_TRIGGER_COLS does, flagged by COLUMN_LIST = 'YES'. It is aggregated in a
    // derived table so that TRIGGER_BODY stays out of any grouped or nested select list. Every view is
    // filtered on the owner and table name directly, rather than leaving the optimizer to derive the
    // predicates through the joins.
    internal const string Sql = $"""

select
    t.TEMPORARY as "{nameof(Result.IsTemporary)}",
    t.PARTITIONED as "{nameof(Result.IsPartitioned)}",
    t.IOT_TYPE as "{nameof(Result.IotType)}",
    t.LOGGING as "{nameof(Result.Logging)}",
    t.DEFAULT_COLLATION as "{nameof(Result.DefaultCollation)}",
    pt.PARTITIONING_TYPE as "{nameof(Result.PartitioningType)}",
    case when et.TABLE_NAME is null then 'N' else 'Y' end as "{nameof(Result.IsExternal)}",
    tr.OWNER as "{nameof(Result.TriggerSchema)}",
    tr.TRIGGER_NAME as "{nameof(Result.TriggerName)}",
    tr.TRIGGER_TYPE as "{nameof(Result.TriggerType)}",
    tr.TRIGGERING_EVENT as "{nameof(Result.TriggerEvent)}",
    tr.WHEN_CLAUSE as "{nameof(Result.TriggerCondition)}",
    tc.UPDATE_COLUMNS as "{nameof(Result.TriggerUpdateColumns)}",
    tr.TRIGGER_BODY as "{nameof(Result.TriggerDefinition)}",
    tr.STATUS as "{nameof(Result.TriggerEnabledStatus)}"
from SYS.ALL_TABLES t
left join SYS.ALL_PART_TABLES pt on t.OWNER = pt.OWNER and t.TABLE_NAME = pt.TABLE_NAME
left join SYS.ALL_EXTERNAL_TABLES et on t.OWNER = et.OWNER and t.TABLE_NAME = et.TABLE_NAME
left join SYS.ALL_TRIGGERS tr
    on tr.TABLE_OWNER = :{nameof(Query.SchemaName)} and tr.TABLE_NAME = :{nameof(Query.TableName)} and tr.BASE_OBJECT_TYPE = 'TABLE'
left join (
    select
        TRIGGER_OWNER,
        TRIGGER_NAME,
        listagg(COLUMN_NAME, ',') within group (order by COLUMN_NAME) as UPDATE_COLUMNS
    from SYS.ALL_TRIGGER_COLS
    where TABLE_OWNER = :{nameof(Query.SchemaName)} and TABLE_NAME = :{nameof(Query.TableName)} and COLUMN_LIST = 'YES'
    group by TRIGGER_OWNER, TRIGGER_NAME
) tc on tc.TRIGGER_OWNER = tr.OWNER and tc.TRIGGER_NAME = tr.TRIGGER_NAME
where t.OWNER = :{nameof(Query.SchemaName)} and t.TABLE_NAME = :{nameof(Query.TableName)}
""";
}
