namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableParentKeys
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string ParentTableSchema { get; init; }

        public required string ParentTableName { get; init; }

        public required string ChildKeyName { get; init; }

        public required string ParentKeyName { get; init; }

        public required string ColumnName { get; init; }

        public required int ConstraintColumnId { get; init; }

        public required string ParentKeyType { get; init; }

        public required int DeleteAction { get; init; }

        public required int UpdateAction { get; init; }

        public required bool IsDisabled { get; init; }
    }

    internal const string Sql = @$"
select
    schema_name(parent_t.schema_id) as [{nameof(Result.ParentTableSchema)}],
    parent_t.name as [{nameof(Result.ParentTableName)}],
    fk.name as [{nameof(Result.ChildKeyName)}],
    c.name as [{nameof(Result.ColumnName)}],
    fkc.constraint_column_id as [{nameof(Result.ConstraintColumnId)}],
    kc.name as [{nameof(Result.ParentKeyName)}],
    kc.type as [{nameof(Result.ParentKeyType)}],
    fk.delete_referential_action as [{nameof(Result.DeleteAction)}],
    fk.update_referential_action as [{nameof(Result.UpdateAction)}],
    fk.is_disabled as [{nameof(Result.IsDisabled)}]
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(child_t.schema_id) = @{nameof(Query.SchemaName)} and child_t.name = @{nameof(Query.TableName)}
     and child_t.is_ms_shipped = 0 and parent_t.is_ms_shipped = 0";
}