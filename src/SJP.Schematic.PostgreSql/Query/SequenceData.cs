namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed record SequenceData
    {
        public string? SchemaName { get; init; }

        public string? SequenceName { get; init; }

        public int CacheSize { get; init; }

        public bool Cycle { get; init; }

        public decimal Increment { get; init; }

        public decimal MinValue { get; init; }

        public decimal MaxValue { get; init; }

        public decimal StartValue { get; init; }
    }
}
