namespace SJP.Schematic.PostgreSql.Query
{
    public class SequenceData
    {
        public int CacheSize { get; set; }

        public bool Cycle { get; set; }

        public decimal Increment { get; set; }

        public decimal MinValue { get; set; }

        public decimal MaxValue { get; set; }

        public decimal StartValue { get; set; }
    }
}
