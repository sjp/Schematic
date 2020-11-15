namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed record ForeignKeyData
    {
        public string ChildKeyName { get; init; } = default!;

        public string ColumnName { get; init; } = default!;

        public string ParentSchemaName { get; init; } = default!;

        public string ParentTableName { get; init; } = default!;

        public int ConstraintColumnId { get; init; }

        public string ParentKeyName { get; init; } = default!;

        public string ParentKeyType { get; init; } = default!;

        public string DeleteAction { get; init; } = default!;

        public string UpdateAction { get; init; } = default!;
    }
}
