namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetSequenceName
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string SequenceName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string SequenceName { get; init; } = default!;
    }

    internal const string Sql = @$"
select sequence_schema as ""{nameof(Result.SchemaName)}"", sequence_name as ""{nameof(Result.SequenceName)}""
from information_schema.sequences
where sequence_schema = @{nameof(Query.SchemaName)} and sequence_name = @{nameof(Query.SequenceName)}
    and sequence_schema not in ('pg_catalog', 'information_schema')
limit 1";
}