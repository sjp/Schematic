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
    ic.is_included_column as [{nameof(GetTableIndexes.Result.IsIncludedColumn)}],
    ic.is_descending_key as [{nameof(GetTableIndexes.Result.IsDescending)}],
    c.name as [{nameof(GetTableIndexes.Result.ColumnName)}]
from sys.views v
inner join sys.indexes i on v.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where v.schema_id = schema_id(@{nameof(Query.SchemaName)}) and v.name = @{nameof(Query.ViewName)} and v.is_ms_shipped = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore
order by ic.index_id, ic.key_ordinal, ic.index_column_id";
}
