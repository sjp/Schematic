namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record GetAllSequenceDefinitionsQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string SequenceName { get; init; } = default!;

        public bool IsCached { get; init; }

        public int? CacheSize { get; init; }

        public bool Cycle { get; init; }

        public decimal Increment { get; init; }

        public decimal MinValue { get; init; }

        public decimal MaxValue { get; init; }

        public decimal StartValue { get; init; }
    }
}
