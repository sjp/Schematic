namespace SJP.Schema.SqlServer.Query
{
    public class ChildKeyData
    {
        public string ChildTableSchema { get; set; }

        public string ChildTableName { get; set; }

        public string ChildKeyName { get; set; }

        public string ColumnName { get; set; }

        public int ConstraintColumnId { get; set; }

        public string ParentKeyName { get; set; }

        public string ParentKeyType { get; set; }
    }
}
