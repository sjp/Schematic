namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetAllSchemas
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string? OwnerName { get; init; }

        public required bool IsSystem { get; init; }
    }

    // information_schema and everything prefixed with pg_ (pg_catalog, pg_toast, the per-session
    // pg_temp_* and pg_toast_temp_* namespaces) belong to the server rather than to a user.
    internal const string Sql = $"""

select
    n.nspname as "{nameof(Result.SchemaName)}",
    pg_catalog.pg_get_userbyid(n.nspowner) as "{nameof(Result.OwnerName)}",
    (n.nspname = 'information_schema' or n.nspname like 'pg\_%') as "{nameof(Result.IsSystem)}"
from pg_catalog.pg_namespace n
order by n.nspname
""";
}
