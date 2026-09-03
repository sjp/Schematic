namespace SJP.Schematic.MySql.Queries;

internal static class GetAllSchemas
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required bool IsSystem { get; init; }
    }

    // MySQL uses 'schema' and 'database' interchangeably and records no owner for either.
    // The four schemas below are the ones the server ships with.
    internal const string Sql = $"""

select
    schema_name as `{nameof(Result.SchemaName)}`,
    schema_name in ('mysql', 'information_schema', 'performance_schema', 'sys') as `{nameof(Result.IsSystem)}`
from information_schema.schemata
order by schema_name
""";
}
