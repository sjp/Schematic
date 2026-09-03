namespace SJP.Schematic.Oracle.Queries;

internal static class GetAllSchemas
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        /// <summary>
        /// <c>ALL_USERS.ORACLE_MAINTAINED</c>, i.e. <c>Y</c> when the schema was created by the
        /// database itself rather than by a user. Requires Oracle 12.1 or later.
        /// </summary>
        public required string? OracleMaintained { get; init; }
    }

    // In Oracle a schema and the user that owns it are the same object, so there is no separate
    // owner to report.
    internal const string Sql = $"""

select
    u.USERNAME as "{nameof(Result.SchemaName)}",
    u.ORACLE_MAINTAINED as "{nameof(Result.OracleMaintained)}"
from SYS.ALL_USERS u
order by u.USERNAME
""";
}
