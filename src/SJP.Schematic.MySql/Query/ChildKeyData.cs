namespace SJP.Schematic.MySql.Query
{
    internal sealed class ChildKeyData
    {
        public string ChildTableSchema { get; set; } = default!;

        public string ChildTableName { get; set; } = default!;

        public string ChildKeyName { get; set; } = default!;

        public string ParentKeyName { get; set; } = default!;

        public string ParentKeyType { get; set; } = default!;

        public string DeleteAction { get; set; } = default!;

        public string UpdateAction { get; set; } = default!;
    }
}
