namespace SJP.Schematic.Oracle.Query
{
    internal sealed record ColumnData
    {
        public string? ColumnName { get; init; }

        public string? ColumnTypeSchema { get; init; }

        public string? ColumnTypeName { get; init; }

        public int DataLength { get; init; }

        public int CharacterLength { get; init; }

        public int Precision { get; init; }

        public int Scale { get; init; }

        public string? Collation { get; init; }

        public string? IsComputed { get; init; }

        public string? DefaultValue { get; init; }
    }
}
