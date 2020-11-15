namespace SJP.Schematic.MySql.Query
{
    internal sealed record IndexColumns
    {
        public string IndexName { get; init; } = default!;

        public bool IsNonUnique { get; init; }

        public int ColumnOrdinal { get; init; }

        public string ColumnName { get; init; } = default!;
    }
}
