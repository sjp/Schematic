namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetAllSequenceDefinitions
{
    internal sealed record Result
    {
        public required string? SchemaName { get; init; }

        public required string? SequenceName { get; init; }

        public required int CacheSize { get; init; }

        public required bool Cycle { get; init; }

        public required decimal Increment { get; init; }

        public required decimal MinValue { get; init; }

        public required decimal MaxValue { get; init; }

        public required decimal StartValue { get; init; }
    }

    internal const string Sql = $"""

select
    schemaname as "{nameof(Result.SchemaName)}",
    sequencename as "{nameof(Result.SequenceName)}",
    start_value as "{nameof(Result.StartValue)}",
    min_value as "{nameof(Result.MinValue)}",
    max_value as "{nameof(Result.MaxValue)}",
    increment_by as "{nameof(Result.Increment)}",
    cycle as "{nameof(Result.Cycle)}",
    cache_size as "{nameof(Result.CacheSize)}"
from pg_catalog.pg_sequences
order by schemaname, sequencename
""";
}