namespace SJP.Schematic.MySql.Query
{
    internal sealed class IndexColumns
    {
        public string? IndexName { get; set; }

        public bool IsNonUnique { get; set; }

        public int ColumnOrdinal { get; set; }

        public string? ColumnName { get; set; }
    }
}
