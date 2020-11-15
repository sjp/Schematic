namespace SJP.Schematic.Oracle.Query
{
    internal sealed record IndexColumns
    {
        public string? IndexOwner { get; init; }

        public string? IndexName { get; init; }

        public int IndexProperty { get; init; }

        public string? Uniqueness { get; init; }

        public string? IsDescending { get; init; }

        public string? ColumnName { get; init; }

        public int ColumnPosition { get; init; }
    }
}
