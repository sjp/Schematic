namespace SJP.Schematic.Oracle.Query
{
    internal class ForeignKeyData
    {
        public string ConstraintName { get; set; } = default!;

        public string EnabledStatus { get; set; } = default!;

        public string DeleteAction { get; set; } = default!;

        public string? ParentTableSchema { get; set; } = default!;

        public string? ParentTableName { get; set; } = default!;

        public string? ParentConstraintName { get; set; } = default!;

        public string ParentKeyType { get; set; } = default!;

        public string ColumnName { get; set; } = default!;

        public int ColumnPosition { get; set; }
    }
}
