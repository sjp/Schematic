using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetViewIndexes
{
    // The shape of an index is the same whether it is attached to a table or a view, so the rows are
    // described by GetTableIndexes.Result and mapped by the same code.
    internal sealed record Query : ISqlQuery<GetTableIndexes.Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select
    i.name as [{nameof(GetTableIndexes.Result.IndexName)}],
    i.is_unique as [{nameof(GetTableIndexes.Result.IsUnique)}],
    i.is_disabled as [{nameof(GetTableIndexes.Result.IsDisabled)}],
    i.has_filter as [{nameof(GetTableIndexes.Result.IsFiltered)}],
    i.filter_definition as [{nameof(GetTableIndexes.Result.FilterDefinition)}],
    i.is_primary_key as [{nameof(GetTableIndexes.Result.IsPrimaryKey)}],
    i.is_unique_constraint as [{nameof(GetTableIndexes.Result.IsUniqueConstraint)}],
    i.type as [{nameof(GetTableIndexes.Result.IndexType)}],
    i.fill_factor as [{nameof(GetTableIndexes.Result.FillFactor)}],
    ic.key_ordinal as [{nameof(GetTableIndexes.Result.KeyOrdinal)}],
    ic.index_column_id as [{nameof(GetTableIndexes.Result.IndexColumnId)}],
    -- a columnstore index has no key, so every column it stores is reported as included; those
    -- columns are what CREATE COLUMNSTORE INDEX lists, so they are treated as its columns here
    case when i.type in (5, 6) then cast(0 as bit) else ic.is_included_column end as [{nameof(GetTableIndexes.Result.IsIncludedColumn)}],
    ic.is_descending_key as [{nameof(GetTableIndexes.Result.IsDescending)}],
    c.name as [{nameof(GetTableIndexes.Result.ColumnName)}]
from sys.views v
inner join sys.indexes i on v.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where v.schema_id = schema_id(@{nameof(Query.SchemaName)}) and v.name = @{nameof(Query.ViewName)} and v.is_ms_shipped = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore
    -- a partitioning column that SQL Server adds to an aligned index by itself is neither a key nor an
    -- included column, but would otherwise be read as a key column ahead of every declared one
    and not (ic.key_ordinal = 0 and ic.is_included_column = 0 and ic.partition_ordinal > 0)
order by i.index_id";
}
