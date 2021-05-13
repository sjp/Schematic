namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetAllSequencesQueryResult
    {
        public string? SchemaName { get; init; }

        public string? SequenceName { get; init; }

        public int CacheSize { get; init; }

        public string? Cycle { get; init; }

        public decimal Increment { get; init; }

        public decimal MinValue { get; init; }

        public decimal MaxValue { get; init; }
    }
}
