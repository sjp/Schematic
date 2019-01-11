namespace SJP.Schematic.MySql.Query
{
    internal class ColumnData
    {
        public string ColumnName { get; set; }

        public string DataTypeName { get; set; }

        public int CharacterMaxLength { get; set; }

        public int Precision { get; set; }

        public int Scale { get; set; }

        public int DateTimePrecision { get; set; }

        public string Collation { get; set; }

        public string IsNullable { get; set; }

        public string DefaultValue { get; set; }

        public string ComputedColumnDefinition { get; set; }

        public string ExtraInformation { get; set; }
    }
}
