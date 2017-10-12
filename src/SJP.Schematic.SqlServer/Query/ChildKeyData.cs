namespace SJP.Schematic.SqlServer.Query
{
    public class ChildKeyData
    {
        public string ChildTableSchema { get; set; }

        public string ChildTableName { get; set; }

        public string ChildKeyName { get; set; }

        public string ParentKeyName { get; set; }

        public string ParentKeyType { get; set; }

        public int DeleteRule { get; set; }

        public int UpdateRule { get; set; }
    }
}
