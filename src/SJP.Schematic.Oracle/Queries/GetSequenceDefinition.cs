using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetSequenceDefinition
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string SequenceName { get; init; }
    }

    internal sealed record Result
    {
        public required int CacheSize { get; init; }

        public required string? Cycle { get; init; }

        public required decimal Increment { get; init; }

        public required decimal MinValue { get; init; }

        public required decimal MaxValue { get; init; }
    }

    internal const string Sql = @$"
select
    INCREMENT_BY as ""{nameof(Result.Increment)}"",
    MIN_VALUE as ""{nameof(Result.MinValue)}"",
    MAX_VALUE as ""{nameof(Result.MaxValue)}"",
    CYCLE_FLAG as ""{nameof(Result.Cycle)}"",
    CACHE_SIZE as ""{nameof(Result.CacheSize)}""
from SYS.ALL_SEQUENCES
where SEQUENCE_OWNER = :{nameof(Query.SchemaName)} and SEQUENCE_NAME = :{nameof(Query.SequenceName)}";
}