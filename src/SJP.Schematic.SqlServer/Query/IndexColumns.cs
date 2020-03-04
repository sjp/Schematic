namespace SJP.Schematic.SqlServer.Query
{
    internal class IndexColumns
    {
        public string IndexName { get; set; } = default!;

        public bool IsUnique { get; set; }

        public int KeyOrdinal { get; set; }

        public int IndexColumnId { get; set; }

        public bool IsIncludedColumn { get; set; }

        public bool IsDescending { get; set; }

        public string ColumnName { get; set; } = default!;

        public bool IsDisabled { get; set; }
    }
}
