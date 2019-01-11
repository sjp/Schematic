namespace SJP.Schematic.Oracle.Query
{
    internal class IndexColumns
    {
        public string IndexOwner { get; set; }

        public string IndexName { get; set; }

        public int IndexProperty { get; set; }

        public string Uniqueness { get; set; }

        public string IsDescending { get; set; }

        public string ColumnName { get; set; }

        public int ColumnPosition { get; set; }
    }
}
