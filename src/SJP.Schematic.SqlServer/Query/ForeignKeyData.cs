namespace SJP.Schematic.SqlServer.Query
{
    internal class ForeignKeyData
    {
        public string ParentTableSchema { get; set; }

        public string ParentTableName { get; set; }

        public string ChildKeyName { get; set; }

        public string ParentKeyName { get; set; }

        public string ColumnName { get; set; }

        public int ConstraintColumnId { get; set; }

        public string ParentKeyType { get; set; }

        public int DeleteRule { get; set; }

        public int UpdateRule { get; set; }

        public bool IsDisabled { get; set; }
    }
}
