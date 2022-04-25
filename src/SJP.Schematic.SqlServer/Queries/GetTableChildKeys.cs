namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableChildKeys
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string ChildTableSchema { get; init; } = default!;

        public string ChildTableName { get; init; } = default!;

        public string ChildKeyName { get; init; } = default!;

        public string ParentKeyName { get; init; } = default!;

        public string ParentKeyType { get; init; } = default!;

        public int DeleteAction { get; init; }

        public int UpdateAction { get; init; }
    }

    internal const string Sql = @$"
select
    schema_name(child_t.schema_id) as [{ nameof(Result.ChildTableSchema) }],
    child_t.name as [{ nameof(Result.ChildTableName) }],
    fk.name as [{ nameof(Result.ChildKeyName) }],
    kc.name as [{ nameof(Result.ParentKeyName) }],
    kc.type as [{ nameof(Result.ParentKeyType) }],
    fk.delete_referential_action as [{ nameof(Result.DeleteAction) }],
    fk.update_referential_action as [{ nameof(Result.UpdateAction) }]
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(parent_t.schema_id) = @{ nameof(Query.SchemaName) } and parent_t.name = @{ nameof(Query.TableName) }
    and child_t.is_ms_shipped = 0 and parent_t.is_ms_shipped = 0";
}