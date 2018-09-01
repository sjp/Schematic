namespace SJP.Schematic.Oracle.Query
{
    public class ColumnData
    {
        public string ColumnName { get; set; }

        public string ColumnTypeSchema { get; set; }

        public string ColumnTypeName { get; set; }

        public int DataLength { get; set; }

        public int CharacterLength { get; set; }

        public int Precision { get; set; }

        public int Scale { get; set; }

        public string Collation { get; set; }

        public string IsComputed { get; set; }

        public string DefaultValue { get; set; }
    }
}
