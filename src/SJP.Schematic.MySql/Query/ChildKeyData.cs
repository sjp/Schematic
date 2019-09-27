namespace SJP.Schematic.MySql.Query
{
    internal class ChildKeyData
    {
        public string? ChildTableSchema { get; set; }

        public string? ChildTableName { get; set; }

        public string? ChildKeyName { get; set; }

        public string? ParentKeyName { get; set; }

        public string? ParentKeyType { get; set; }

        public string? DeleteAction { get; set; }

        public string? UpdateAction { get; set; }
    }
}
