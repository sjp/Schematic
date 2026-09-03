using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableOptions
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

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
    }

    // A table is partitioned when the heap or clustered index that stores it lives on a partition
    // scheme rather than a filegroup. SQL Server permits only one partitioning column, so the join
    // to sys.index_columns matches at most one row.
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
    pc.name as [{nameof(Result.PartitionColumnName)}]
from sys.tables t
left join sys.tables ht on ht.object_id = t.history_table_id
left join sys.periods p on p.object_id = t.object_id and p.period_type = 1
left join sys.columns psc on psc.object_id = t.object_id and psc.column_id = p.start_column_id
left join sys.columns pec on pec.object_id = t.object_id and pec.column_id = p.end_column_id
left join sys.indexes i on i.object_id = t.object_id and i.index_id in (0, 1)
left join sys.partition_schemes ps on ps.data_space_id = i.data_space_id
left join sys.index_columns pic on pic.object_id = i.object_id and pic.index_id = i.index_id and pic.partition_ordinal > 0
left join sys.columns pc on pc.object_id = pic.object_id and pc.column_id = pic.column_id
where t.schema_id = schema_id(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0";
}
