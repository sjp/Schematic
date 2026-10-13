using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetSequenceName
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string SequenceName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string SequenceName { get; init; }
    }

    // Reads the catalogs rather than pg_sequences: that view computes last_value for every row,
    // which locks and reads each sequence and can exhaust the lock table on large databases.
    internal const string Sql = $"""

select n.nspname as "{nameof(Result.SchemaName)}", c.relname as "{nameof(Result.SequenceName)}"
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on n.oid = c.relnamespace
where n.nspname = @{nameof(Query.SchemaName)} and c.relname = @{nameof(Query.SequenceName)}
    and c.relkind = 'S'
    and n.nspname not in ('pg_catalog', 'information_schema')
    and not pg_catalog.pg_is_other_temp_schema(n.oid)
limit 1
""";
}