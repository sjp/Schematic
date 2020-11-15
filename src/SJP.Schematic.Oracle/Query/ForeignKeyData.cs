namespace SJP.Schematic.Oracle.Query
{
    internal sealed record ForeignKeyData
    {
        public string ConstraintName { get; init; } = default!;

        public string EnabledStatus { get; init; } = default!;

        public string DeleteAction { get; init; } = default!;

        public string? ParentTableSchema { get; init; } = default!;

        public string? ParentTableName { get; init; } = default!;

        public string? ParentConstraintName { get; init; } = default!;

        public string ParentKeyType { get; init; } = default!;

        public string ColumnName { get; init; } = default!;

        public int ColumnPosition { get; init; }
    }
}
