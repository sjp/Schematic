namespace SJP.Schematic.Oracle.Queries;

internal static class GetAllSequences
{
    internal sealed record Result
    {
        public required string? SchemaName { get; init; }

        public required string? SequenceName { get; init; }

        public required int CacheSize { get; init; }

        public required string? Cycle { get; init; }

        public required decimal Increment { get; init; }

        public required decimal MinValue { get; init; }

        public required decimal MaxValue { get; init; }
    }

    internal const string Sql = @$"
select
    s.SEQUENCE_OWNER as ""{nameof(Result.SchemaName)}"",
    s.SEQUENCE_NAME as ""{nameof(Result.SequenceName)}"",
    INCREMENT_BY as ""{nameof(Result.Increment)}"",
    MIN_VALUE as ""{nameof(Result.MinValue)}"",
    MAX_VALUE as ""{nameof(Result.MaxValue)}"",
    CYCLE_FLAG as ""{nameof(Result.Cycle)}"",
    CACHE_SIZE as ""{nameof(Result.CacheSize)}""
from SYS.ALL_SEQUENCES s
inner join SYS.ALL_OBJECTS o on s.SEQUENCE_OWNER = o.OWNER and s.SEQUENCE_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by s.SEQUENCE_OWNER, s.SEQUENCE_NAME";
}