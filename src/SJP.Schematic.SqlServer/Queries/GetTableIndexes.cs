using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableIndexes
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string IndexName { get; init; }

        public required bool IsUnique { get; init; }

        public required int KeyOrdinal { get; init; }

        public required int IndexColumnId { get; init; }

        public required bool IsIncludedColumn { get; init; }

        public required bool IsDescending { get; init; }

        public required string ColumnName { get; init; }

        public required bool IsDisabled { get; init; }

        public required bool IsFiltered { get; init; }

        public required string FilterDefinition { get; init; }

        public required bool IsPrimaryKey { get; init; }

        public required bool IsUniqueConstraint { get; init; }

        /// <summary>
        /// The <c>sys.indexes.type</c> value, e.g. <c>1</c> for a clustered rowstore index.
        /// </summary>
        public required int IndexType { get; init; }

        /// <summary>
        /// The index fill factor, where <c>0</c> means the server default is used.
        /// </summary>
        public required int FillFactor { get; init; }

        /// <summary>
        /// The position of the column in the sort order of an ordered columnstore index, or <c>0</c> when the
        /// column is not part of that order. Not read by the index query, because the catalog only has it
        /// from SQL Server 2022; it is filled in from <see cref="GetTableColumnStoreOrder"/> when needed.
        /// </summary>
        public int ColumnStoreOrderOrdinal { get; init; }
    }

    internal const string Sql = @$"
select
    i.name as [{nameof(Result.IndexName)}],
    i.is_unique as [{nameof(Result.IsUnique)}],
    i.is_disabled as [{nameof(Result.IsDisabled)}],
    i.has_filter as [{nameof(Result.IsFiltered)}],
    i.filter_definition as [{nameof(Result.FilterDefinition)}],
    i.is_primary_key as [{nameof(Result.IsPrimaryKey)}],
    i.is_unique_constraint as [{nameof(Result.IsUniqueConstraint)}],
    i.type as [{nameof(Result.IndexType)}],
    i.fill_factor as [{nameof(Result.FillFactor)}],
    ic.key_ordinal as [{nameof(Result.KeyOrdinal)}],
    ic.index_column_id as [{nameof(Result.IndexColumnId)}],
    -- a columnstore index has no key, so every column it stores is reported as included; those
    -- columns are what CREATE COLUMNSTORE INDEX lists, so they are treated as its columns here
    case when i.type in (5, 6) then cast(0 as bit) else ic.is_included_column end as [{nameof(Result.IsIncludedColumn)}],
    ic.is_descending_key as [{nameof(Result.IsDescending)}],
    c.name as [{nameof(Result.ColumnName)}]
from sys.tables t
inner join sys.indexes i on t.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where t.schema_id = schema_id(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore
    -- a partitioning column that SQL Server adds to an aligned index by itself is neither a key nor an
    -- included column, but would otherwise be read as a key column ahead of every declared one
    and not (ic.key_ordinal = 0 and ic.is_included_column = 0 and ic.partition_ordinal > 0)
order by i.index_id";
}