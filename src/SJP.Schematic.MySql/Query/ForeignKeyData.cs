namespace SJP.Schematic.MySql.Query
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

        public string DeleteAction { get; set; } = default!;

        public string UpdateAction { get; set; } = default!;
    }
}
