namespace SJP.Schematic.SqlServer.Query
{
    internal class ChildKeyData
    {
        public string ChildTableSchema { get; set; } = default!;

        public string ChildTableName { get; set; } = default!;

        public string ChildKeyName { get; set; } = default!;

        public string ParentKeyName { get; set; } = default!;

        public string ParentKeyType { get; set; } = default!;

        public int DeleteAction { get; set; }

        public int UpdateAction { get; set; }
    }
}
