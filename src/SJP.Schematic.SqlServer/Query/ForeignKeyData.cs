namespace SJP.Schematic.SqlServer.Query
{
    internal class ForeignKeyData
    {
        public string ParentTableSchema { get; set; } = default!;

        public string ParentTableName { get; set; } = default!;

        public string ChildKeyName { get; set; } = default!;

        public string ParentKeyName { get; set; } = default!;

        public string ColumnName { get; set; } = default!;

        public int ConstraintColumnId { get; set; }

        public string ParentKeyType { get; set; } = default!;

        public int DeleteAction { get; set; }

        public int UpdateAction { get; set; }

        public bool IsDisabled { get; set; }
    }
}
