using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableDetails
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    /// <summary>
    /// A table's options, together with one of its check constraints or triggers. The options repeat on
    /// every row; a table with neither checks nor triggers has a single row with no <see cref="ObjectKind"/>.
    /// </summary>
    internal sealed record Result
    {
        /// <summary>0 = not temporal, 1 = a history table, 2 = a system-versioned table.</summary>
        public required int TemporalType { get; init; }

        public required string? HistoryTableSchemaName { get; init; }

        public required string? HistoryTableName { get; init; }

        public required string? PeriodStartColumnName { get; init; }

        public required string? PeriodEndColumnName { get; init; }

        /// <summary>0 = <c>SCHEMA_AND_DATA</c>, 1 = <c>SCHEMA_ONLY</c>.</summary>
        public required int Durability { get; init; }

        public required bool IsExternal { get; init; }

        public required string? PartitionSchemeName { get; init; }

        public required string? PartitionColumnName { get; init; }

        /// <summary>
        /// <see cref="CheckObjectKind"/> for a check constraint, <see cref="TriggerObjectKind"/> for a trigger,
        /// or <see langword="null" /> when the table has neither.
        /// </summary>
        public required string? ObjectKind { get; init; }

        public required string? ObjectName { get; init; }

        /// <summary>
        /// The check expression or trigger body. A trigger's is <see langword="null" /> when it was created
        /// <c>WITH ENCRYPTION</c> or the caller lacks <c>VIEW DEFINITION</c> on it.
        /// </summary>
        public required string? Definition { get; init; }

        public required bool? IsDisabled { get; init; }

        /// <summary>
        /// Set for a check constraint created or re-enabled <c>WITH NOCHECK</c>, i.e. SQL Server has not
        /// verified the existing rows against it.
        /// </summary>
        public required bool? IsNotTrusted { get; init; }

        public required bool? IsInsteadOfTrigger { get; init; }

        public required bool? IsInsertTrigger { get; init; }

        public required bool? IsUpdateTrigger { get; init; }

        public required bool? IsDeleteTrigger { get; init; }

        /// <summary>
        /// Set when a trigger fires on an event other than <c>INSERT</c>, <c>UPDATE</c> or <c>DELETE</c>.
        /// </summary>
        public required bool? IsOtherTrigger { get; init; }
    }

    internal const string CheckObjectKind = "C";

    internal const string TriggerObjectKind = "T";

    // A table is partitioned when the heap or clustered index that stores it lives on a partition
    // scheme rather than a filegroup. SQL Server permits only one partitioning column, so the join
    // to sys.index_columns matches at most one row, and the options joins return one row per table.
    //
    // The outer apply multiplies that row by the table's checks and triggers, and keeps it when there
    // are none. sys.trigger_events emits one row per (trigger, event), and a trigger fired on multiple
    // events would otherwise resend its (potentially large) definition once per event. The events are
    // pivoted per trigger inside the inner apply, so the definition is never part of a grouping key; the
    // group by makes the apply return no row for a trigger without events, which excludes it like an
    // inner join.
    internal const string Sql = @$"
select
    t.temporal_type as [{nameof(Result.TemporalType)}],
    schema_name(ht.schema_id) as [{nameof(Result.HistoryTableSchemaName)}],
    ht.name as [{nameof(Result.HistoryTableName)}],
    psc.name as [{nameof(Result.PeriodStartColumnName)}],
    pec.name as [{nameof(Result.PeriodEndColumnName)}],
    t.durability as [{nameof(Result.Durability)}],
    t.is_external as [{nameof(Result.IsExternal)}],
    ps.name as [{nameof(Result.PartitionSchemeName)}],
    pc.name as [{nameof(Result.PartitionColumnName)}],
    x.ObjectKind as [{nameof(Result.ObjectKind)}],
    x.ObjectName as [{nameof(Result.ObjectName)}],
    x.Definition as [{nameof(Result.Definition)}],
    x.IsDisabled as [{nameof(Result.IsDisabled)}],
    x.IsNotTrusted as [{nameof(Result.IsNotTrusted)}],
    x.IsInsteadOfTrigger as [{nameof(Result.IsInsteadOfTrigger)}],
    x.IsInsertTrigger as [{nameof(Result.IsInsertTrigger)}],
    x.IsUpdateTrigger as [{nameof(Result.IsUpdateTrigger)}],
    x.IsDeleteTrigger as [{nameof(Result.IsDeleteTrigger)}],
    x.IsOtherTrigger as [{nameof(Result.IsOtherTrigger)}]
from sys.tables t
left join sys.tables ht on ht.object_id = t.history_table_id
left join sys.periods p on p.object_id = t.object_id and p.period_type = 1
left join sys.columns psc on psc.object_id = t.object_id and psc.column_id = p.start_column_id
left join sys.columns pec on pec.object_id = t.object_id and pec.column_id = p.end_column_id
left join sys.indexes i on i.object_id = t.object_id and i.index_id in (0, 1)
left join sys.partition_schemes ps on ps.data_space_id = i.data_space_id
left join sys.index_columns pic on pic.object_id = i.object_id and pic.index_id = i.index_id and pic.partition_ordinal > 0
left join sys.columns pc on pc.object_id = pic.object_id and pc.column_id = pic.column_id
outer apply (
    select
        '{CheckObjectKind}' as ObjectKind,
        cc.name as ObjectName,
        cc.definition as Definition,
        cc.is_disabled as IsDisabled,
        cc.is_not_trusted as IsNotTrusted,
        cast(null as bit) as IsInsteadOfTrigger,
        cast(null as bit) as IsInsertTrigger,
        cast(null as bit) as IsUpdateTrigger,
        cast(null as bit) as IsDeleteTrigger,
        cast(null as bit) as IsOtherTrigger
    from sys.check_constraints cc
    where cc.parent_object_id = t.object_id
    union all
    select
        '{TriggerObjectKind}',
        st.name,
        sm.definition,
        st.is_disabled,
        cast(null as bit),
        st.is_instead_of_trigger,
        ev.IsInsertTrigger,
        ev.IsUpdateTrigger,
        ev.IsDeleteTrigger,
        ev.IsOtherTrigger
    from sys.triggers st
    inner join sys.sql_modules sm on sm.object_id = st.object_id
    cross apply (
        select
            cast(max(case when te.type_desc = 'INSERT' then 1 else 0 end) as bit) as IsInsertTrigger,
            cast(max(case when te.type_desc = 'UPDATE' then 1 else 0 end) as bit) as IsUpdateTrigger,
            cast(max(case when te.type_desc = 'DELETE' then 1 else 0 end) as bit) as IsDeleteTrigger,
            cast(max(case when te.type_desc not in ('INSERT', 'UPDATE', 'DELETE') then 1 else 0 end) as bit) as IsOtherTrigger
        from sys.trigger_events te
        where te.object_id = st.object_id
        group by te.object_id
    ) ev
    where st.parent_id = t.object_id
) x
where t.schema_id = schema_id(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0";
}
