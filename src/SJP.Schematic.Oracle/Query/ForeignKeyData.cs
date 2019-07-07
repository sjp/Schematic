namespace SJP.Schematic.Oracle.Query
{
    internal class ForeignKeyData
    {
        public string ConstraintName { get; set; }

        public string EnabledStatus { get; set; }

        public string DeleteAction { get; set; }

        public string ParentTableSchema { get; set; }

        public string ParentTableName { get; set; }

        public string ParentConstraintName { get; set; }

        public string ParentKeyType { get; set; }

        public string ColumnName { get; set; }

        public int ColumnPosition { get; set; }
    }
}
