namespace SJP.Schematic.Oracle.Query
{
    internal sealed record SequenceData
    {
        public string? SchemaName { get; init; }

        public string? ObjectName { get; init; }

        public int CacheSize { get; init; }

        public string? Cycle { get; init; }

        public decimal Increment { get; init; }

        public decimal MinValue { get; init; }

        public decimal MaxValue { get; init; }
    }
}
