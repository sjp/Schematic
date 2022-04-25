namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableIndexes
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string IndexName { get; init; } = default!;

        public bool IsUnique { get; init; }

        public int KeyOrdinal { get; init; }

        public int IndexColumnId { get; init; }

        public bool IsIncludedColumn { get; init; }

        public bool IsDescending { get; init; }

        public string ColumnName { get; init; } = default!;

        public bool IsDisabled { get; init; }
    }

    internal const string Sql = @$"
select
    i.name as [{ nameof(Result.IndexName) }],
    i.is_unique as [{ nameof(Result.IsUnique) }],
    ic.key_ordinal as [{ nameof(Result.KeyOrdinal) }],
    ic.index_column_id as [{ nameof(Result.IndexColumnId) }],
    ic.is_included_column as [{ nameof(Result.IsIncludedColumn) }],
    ic.is_descending_key as [{ nameof(Result.IsDescending) }],
    c.name as [{ nameof(Result.ColumnName) }],
    i.is_disabled as [{ nameof(Result.IsDisabled) }]
from sys.tables t
inner join sys.indexes i on t.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where schema_name(t.schema_id) = @{ nameof(Query.SchemaName) } and t.name = @{ nameof(Query.TableName) } and t.is_ms_shipped = 0
    and i.is_primary_key = 0 and i.is_unique_constraint = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore
order by ic.index_id, ic.key_ordinal, ic.index_column_id";
}