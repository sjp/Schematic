namespace SJP.Schematic.PostgreSql.Query
{
    internal class SequenceData
    {
        public string SchemaName { get; set; }

        public string SequenceName { get; set; }

        public int CacheSize { get; set; }

        public bool Cycle { get; set; }

        public decimal Increment { get; set; }

        public decimal MinValue { get; set; }

        public decimal MaxValue { get; set; }

        public decimal StartValue { get; set; }
    }
}
