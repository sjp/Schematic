namespace SJP.Schematic.SqlServer.Query
{
    public class IndexColumns
    {
        public string IndexName { get; set; }

        public bool IsUnique { get; set; }

        public int KeyOrdinal { get; set; }

        public int IndexColumnId { get; set; }

        public bool IsIncludedColumn { get; set; }

        public bool IsDescending { get; set; }

        public string ColumnName { get; set; }

        public bool IsDisabled { get; set; }
    }
}
