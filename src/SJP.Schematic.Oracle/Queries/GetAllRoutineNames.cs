namespace SJP.Schematic.Oracle.Queries;

internal static class GetAllRoutineNames
{
    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }

    internal const string Sql = @$"
SELECT
    OWNER as ""{ nameof(Result.SchemaName) }"",
    OBJECT_NAME as ""{ nameof(Result.RoutineName) }""
FROM SYS.ALL_OBJECTS
WHERE ORACLE_MAINTAINED <> 'Y' AND OBJECT_TYPE in ('FUNCTION', 'PROCEDURE')
ORDER BY OWNER, OBJECT_NAME";
}