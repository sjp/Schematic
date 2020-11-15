namespace SJP.Schematic.Oracle.Query
{
    internal sealed record ChildKeyData
    {
        public string? ChildTableSchema { get; init; }

        public string? ChildTableName { get; init; }

        public string? ChildKeyName { get; init; }

        public string? EnabledStatus { get; init; }

        public string? DeleteAction { get; init; }

        public string? ParentKeyName { get; init; }

        public string? ParentKeyType { get; init; }
    }
}
