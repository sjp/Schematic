namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetAllSequenceDefinitions
{
    internal sealed record Result
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

    internal const string Sql = @$"
select
    schemaname as ""{nameof(Result.SchemaName)}"",
    sequencename as ""{nameof(Result.SequenceName)}"",
    start_value as ""{nameof(Result.StartValue)}"",
    min_value as ""{nameof(Result.MinValue)}"",
    max_value as ""{nameof(Result.MaxValue)}"",
    increment_by as ""{nameof(Result.Increment)}"",
    cycle as ""{nameof(Result.Cycle)}"",
    cache_size as ""{nameof(Result.CacheSize)}""
from pg_catalog.pg_sequences
order by schemaname, sequencename";
}