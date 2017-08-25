namespace SJP.Schema.SqlServer.Query
{
    public class ForeignKeyData
    {
        public string ParentTableSchema { get; set; }

        public string ParentTableName { get; set; }

        public string ChildKeyName { get; set; }

        public string ParentKeyName { get; set; }

        public string ColumnName { get; set; }

        public int ConstraintColumnId { get; set; }

        public string ParentKeyType { get; set; }

        public int DeleteAction { get; set; }

        public int UpdateAction { get; set; }

        public bool IsDisabled { get; set; }
    }
}
