using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableForeignKeys
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    /// <summary>
    /// A foreign key at either end of a table: one the table owns, with one row per key column, or one
    /// on another table that references it, with one row per key. A self-referencing foreign key is both.
    /// </summary>
    internal sealed record Result
    {
        /// <summary>
        /// Set when the queried table owns the foreign key, i.e. the row describes one of its parent keys;
        /// otherwise the row describes a child table's foreign key that references the queried table.
        /// </summary>
        public required bool IsParentKey { get; init; }

        public required string ParentTableSchema { get; init; }

        public required string ParentTableName { get; init; }

        public required string ChildTableSchema { get; init; }

        public required string ChildTableName { get; init; }

        public required string ChildKeyName { get; init; }

        /// <summary>The foreign key column, or <see langword="null" /> when <see cref="IsParentKey"/> is not set.</summary>
        public required string? ColumnName { get; init; }

        /// <summary>The foreign key column's position, or <see langword="null" /> when <see cref="IsParentKey"/> is not set.</summary>
        public required int? ConstraintColumnId { get; init; }

        public required string ParentKeyName { get; init; }

        public required string ParentKeyType { get; init; }

        public required int DeleteAction { get; init; }

        public required int UpdateAction { get; init; }

        public required bool IsDisabled { get; init; }

        /// <summary>
        /// Set when the foreign key was created or re-enabled <c>WITH NOCHECK</c>, i.e. SQL Server has
        /// not verified the existing rows against it.
        /// </summary>
        public required bool IsNotTrusted { get; init; }
    }

    // The first branch reads the foreign keys the table owns, with their columns; the second reads the
    // foreign keys that reference the table, which need no columns because the referencing table's own
    // load resolves them.
    //
    // The referenced key may be a unique constraint (sys.key_constraints) or a bare unique index with no
    // backing constraint; sys.indexes covers both, so it's the authoritative source and kc is an optional enrichment.
    internal const string Sql = @$"
select
    cast(1 as bit) as [{nameof(Result.IsParentKey)}],
    schema_name(parent_t.schema_id) as [{nameof(Result.ParentTableSchema)}],
    parent_t.name as [{nameof(Result.ParentTableName)}],
    schema_name(child_t.schema_id) as [{nameof(Result.ChildTableSchema)}],
    child_t.name as [{nameof(Result.ChildTableName)}],
    fk.name as [{nameof(Result.ChildKeyName)}],
    c.name as [{nameof(Result.ColumnName)}],
    fkc.constraint_column_id as [{nameof(Result.ConstraintColumnId)}],
    coalesce(kc.name, pi.name) as [{nameof(Result.ParentKeyName)}],
    coalesce(kc.type, 'UQ') as [{nameof(Result.ParentKeyType)}],
    fk.delete_referential_action as [{nameof(Result.DeleteAction)}],
    fk.update_referential_action as [{nameof(Result.UpdateAction)}],
    fk.is_disabled as [{nameof(Result.IsDisabled)}],
    fk.is_not_trusted as [{nameof(Result.IsNotTrusted)}]
from sys.tables child_t
inner join sys.foreign_keys fk on fk.parent_object_id = child_t.object_id
inner join sys.tables parent_t on parent_t.object_id = fk.referenced_object_id
inner join sys.foreign_key_columns fkc on fkc.constraint_object_id = fk.object_id
inner join sys.columns c on c.object_id = fkc.parent_object_id and c.column_id = fkc.parent_column_id
inner join sys.indexes pi on pi.object_id = fk.referenced_object_id and pi.index_id = fk.key_index_id
left join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where child_t.schema_id = schema_id(@{nameof(Query.SchemaName)}) and child_t.name = @{nameof(Query.TableName)}
    and child_t.is_ms_shipped = 0 and parent_t.is_ms_shipped = 0
union all
select
    cast(0 as bit),
    schema_name(parent_t.schema_id),
    parent_t.name,
    schema_name(child_t.schema_id),
    child_t.name,
    fk.name,
    null,
    null,
    coalesce(kc.name, pi.name),
    coalesce(kc.type, 'UQ'),
    fk.delete_referential_action,
    fk.update_referential_action,
    fk.is_disabled,
    fk.is_not_trusted
from sys.tables parent_t
inner join sys.foreign_keys fk on fk.referenced_object_id = parent_t.object_id
inner join sys.tables child_t on child_t.object_id = fk.parent_object_id
inner join sys.indexes pi on pi.object_id = fk.referenced_object_id and pi.index_id = fk.key_index_id
left join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where parent_t.schema_id = schema_id(@{nameof(Query.SchemaName)}) and parent_t.name = @{nameof(Query.TableName)}
    and child_t.is_ms_shipped = 0 and parent_t.is_ms_shipped = 0";
}
