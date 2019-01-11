namespace SJP.Schematic.MySql.Query
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

        public string DeleteRule { get; set; }

        public string UpdateRule { get; set; }
    }
}
