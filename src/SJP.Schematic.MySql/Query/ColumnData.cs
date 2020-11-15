namespace SJP.Schematic.MySql.Query
{
    internal sealed record ColumnData
    {
        public string ColumnName { get; init; } = default!;

        public string DataTypeName { get; init; } = default!;

        public int CharacterMaxLength { get; init; }

        public int Precision { get; init; }

        public int Scale { get; init; }

        public int DateTimePrecision { get; init; }

        public string? Collation { get; init; }

        public string? IsNullable { get; init; }

        public string? DefaultValue { get; init; }

        public string? ComputedColumnDefinition { get; init; }

        public string? ExtraInformation { get; init; }
    }
}
