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

    internal const string Sql = $"""

select schemaname as "{nameof(Result.SchemaName)}", sequencename as "{nameof(Result.SequenceName)}"
from pg_catalog.pg_sequences
where schemaname = @{nameof(Query.SchemaName)} and sequencename = @{nameof(Query.SequenceName)}
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1
""";
}