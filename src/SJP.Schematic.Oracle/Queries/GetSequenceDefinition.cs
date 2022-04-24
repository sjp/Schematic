namespace SJP.Schematic.Oracle.Queries;

internal static class GetSequenceDefinition
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string SequenceName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public int CacheSize { get; init; }

        public string? Cycle { get; init; }

        public decimal Increment { get; init; }

        public decimal MinValue { get; init; }

        public decimal MaxValue { get; init; }
    }

    internal const string Sql = @$"
select
    INCREMENT_BY as ""{ nameof(Result.Increment) }"",
    MIN_VALUE as ""{ nameof(Result.MinValue) }"",
    MAX_VALUE as ""{ nameof(Result.MaxValue) }"",
    CYCLE_FLAG as ""{ nameof(Result.Cycle) }"",
    CACHE_SIZE as ""{ nameof(Result.CacheSize) }""
from SYS.ALL_SEQUENCES
where SEQUENCE_OWNER = :{ nameof(Query.SchemaName) } and SEQUENCE_NAME = :{ nameof(Query.SequenceName) }";
}