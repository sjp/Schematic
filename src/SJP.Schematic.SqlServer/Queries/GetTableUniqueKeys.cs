namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableUniqueKeys
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string ConstraintName { get; init; } = default!;

        public string ColumnName { get; init; } = default!;

        public bool IsDisabled { get; init; }
    }

    internal const string Sql = @$"
select
    kc.name as [{nameof(Result.ConstraintName)}],
    c.name as [{nameof(Result.ColumnName)}],
    i.is_disabled as [{nameof(Result.IsDisabled)}]
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
inner join sys.indexes i on kc.parent_object_id = i.object_id and kc.unique_index_id = i.index_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where
    schema_name(t.schema_id) = @{nameof(Query.SchemaName)} and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0
    and kc.type = 'UQ'
    and ic.is_included_column = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore
order by ic.key_ordinal";
}