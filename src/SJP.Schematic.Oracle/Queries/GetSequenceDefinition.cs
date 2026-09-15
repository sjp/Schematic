using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetSequenceDefinition
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string SequenceName { get; init; }
    }

    internal sealed record Result : ISequenceDefinitionRow
    {
        public required string SchemaName { get; init; }

        public required string SequenceName { get; init; }

        public required int CacheSize { get; init; }

        public required string? Cycle { get; init; }

        public required string? Order { get; init; }

        public required decimal Increment { get; init; }

        public required decimal MinValue { get; init; }

        public required decimal MaxValue { get; init; }
    }

    internal const string Sql = $"""

select
    s.SEQUENCE_OWNER as "{nameof(Result.SchemaName)}",
    s.SEQUENCE_NAME as "{nameof(Result.SequenceName)}",
    s.INCREMENT_BY as "{nameof(Result.Increment)}",
    s.MIN_VALUE as "{nameof(Result.MinValue)}",
    s.MAX_VALUE as "{nameof(Result.MaxValue)}",
    s.CYCLE_FLAG as "{nameof(Result.Cycle)}",
    s.ORDER_FLAG as "{nameof(Result.Order)}",
    s.CACHE_SIZE as "{nameof(Result.CacheSize)}"
from SYS.ALL_SEQUENCES s
inner join SYS.ALL_OBJECTS o on s.SEQUENCE_OWNER = o.OWNER and s.SEQUENCE_NAME = o.OBJECT_NAME
where s.SEQUENCE_OWNER = :{nameof(Query.SchemaName)} and s.SEQUENCE_NAME = :{nameof(Query.SequenceName)}
    and o.OBJECT_TYPE = 'SEQUENCE' and o.ORACLE_MAINTAINED <> 'Y'
""";
}
