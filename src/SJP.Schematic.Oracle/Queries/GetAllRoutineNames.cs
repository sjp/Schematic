namespace SJP.Schematic.Oracle.Queries;

internal static class GetAllRoutineNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = @$"
SELECT
    OWNER as ""{nameof(Result.SchemaName)}"",
    OBJECT_NAME as ""{nameof(Result.RoutineName)}""
FROM SYS.ALL_OBJECTS
WHERE ORACLE_MAINTAINED <> 'Y' AND OBJECT_TYPE in ('FUNCTION', 'PROCEDURE')
ORDER BY OWNER, OBJECT_NAME";
}