namespace SJP.Schematic.Oracle.Query
{
    internal sealed class SequenceData
    {
        public string? SchemaName { get; set; }

        public string? ObjectName { get; set; }

        public int CacheSize { get; set; }

        public string? Cycle { get; set; }

        public decimal Increment { get; set; }

        public decimal MinValue { get; set; }

        public decimal MaxValue { get; set; }
    }
}
