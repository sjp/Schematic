namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllSequenceDefinitions
{
    internal sealed record Result
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

    internal const string Sql = @$"
select
    schema_name(schema_id) as [{nameof(Result.SchemaName)}],
    name as [{nameof(Result.SequenceName)}],
    start_value as [{nameof(Result.StartValue)}],
    increment as [{nameof(Result.Increment)}],
    minimum_value as [{nameof(Result.MinValue)}],
    maximum_value as [{nameof(Result.MaxValue)}],
    is_cycling as [{nameof(Result.Cycle)}],
    is_cached as [{nameof(Result.IsCached)}],
    cache_size as [{nameof(Result.CacheSize)}]
from sys.sequences
where is_ms_shipped = 0
order by schema_name(schema_id), name";
}