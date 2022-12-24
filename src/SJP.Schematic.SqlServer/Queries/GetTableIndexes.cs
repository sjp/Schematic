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
    }

    internal const string Sql = @$"
select
    i.name as [{nameof(Result.IndexName)}],
    i.is_unique as [{nameof(Result.IsUnique)}],
    i.is_disabled as [{nameof(Result.IsDisabled)}],
    i.has_filter as [{nameof(Result.IsFiltered)}],
    i.filter_definition as [{nameof(Result.FilterDefinition)}],
    ic.key_ordinal as [{nameof(Result.KeyOrdinal)}],
    ic.index_column_id as [{nameof(Result.IndexColumnId)}],
    ic.is_included_column as [{nameof(Result.IsIncludedColumn)}],
    ic.is_descending_key as [{nameof(Result.IsDescending)}],
    c.name as [{nameof(Result.ColumnName)}]
from sys.tables t
inner join sys.indexes i on t.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where schema_name(t.schema_id) = @{nameof(Query.SchemaName)} and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0
    and i.is_primary_key = 0 and i.is_unique_constraint = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore
order by ic.index_id, ic.key_ordinal, ic.index_column_id";
}