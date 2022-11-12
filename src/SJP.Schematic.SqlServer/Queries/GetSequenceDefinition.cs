namespace SJP.Schematic.SqlServer.Queries;

internal static class GetSequenceDefinition
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string SequenceName { get; init; }
    }

    internal sealed record Result
    {
        public required bool IsCached { get; init; }

        public required int? CacheSize { get; init; }

        public required bool Cycle { get; init; }

        public required decimal Increment { get; init; }

        public required decimal MinValue { get; init; }

        public required decimal MaxValue { get; init; }

        public required decimal StartValue { get; init; }
    }

    internal const string Sql = @$"
select
    start_value as [{nameof(Result.StartValue)}],
    increment as [{nameof(Result.Increment)}],
    minimum_value as [{nameof(Result.MinValue)}],
    maximum_value as [{nameof(Result.MaxValue)}],
    is_cycling as [{nameof(Result.Cycle)}],
    is_cached as [{nameof(Result.IsCached)}],
    cache_size as [{nameof(Result.CacheSize)}]
from sys.sequences
where schema_name(schema_id) = @{nameof(Query.SchemaName)}  and name = @{nameof(Query.SequenceName)} and is_ms_shipped = 0";
}