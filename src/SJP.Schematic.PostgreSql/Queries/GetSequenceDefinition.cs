using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

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

        public required string TypeName { get; init; }

        public required int CacheSize { get; init; }

        public required bool Cycle { get; init; }

        public required decimal Increment { get; init; }

        public required decimal MinValue { get; init; }

        public required decimal MaxValue { get; init; }

        public required decimal StartValue { get; init; }
    }

    // Reads the catalogs rather than pg_sequences: that view computes last_value for every row,
    // which locks and reads each sequence and can exhaust the lock table on large databases.
    internal const string Sql = $"""

select
    n.nspname as "{nameof(Result.SchemaName)}",
    c.relname as "{nameof(Result.SequenceName)}",
    pg_catalog.format_type(s.seqtypid, null) as "{nameof(Result.TypeName)}",
    s.seqstart as "{nameof(Result.StartValue)}",
    s.seqmin as "{nameof(Result.MinValue)}",
    s.seqmax as "{nameof(Result.MaxValue)}",
    s.seqincrement as "{nameof(Result.Increment)}",
    s.seqcycle as "{nameof(Result.Cycle)}",
    s.seqcache as "{nameof(Result.CacheSize)}"
from pg_catalog.pg_sequence s
inner join pg_catalog.pg_class c on c.oid = s.seqrelid
inner join pg_catalog.pg_namespace n on n.oid = c.relnamespace
where n.nspname = @{nameof(Query.SchemaName)} and c.relname = @{nameof(Query.SequenceName)}
    and n.nspname not in ('pg_catalog', 'information_schema')
    and not pg_catalog.pg_is_other_temp_schema(n.oid)
limit 1
""";
}
