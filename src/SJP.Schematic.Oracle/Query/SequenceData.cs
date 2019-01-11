namespace SJP.Schematic.Oracle.Query
{
    internal class SequenceData
    {
        public int CacheSize { get; set; }

        public string Cycle { get; set; }

        public decimal Increment { get; set; }

        public decimal MinValue { get; set; }

        public decimal MaxValue { get; set; }
    }
}
