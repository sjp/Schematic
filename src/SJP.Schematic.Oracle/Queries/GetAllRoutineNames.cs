namespace SJP.Schematic.Oracle.Queries;

internal static class GetAllRoutineNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = $"""

select
    OWNER as "{nameof(Result.SchemaName)}",
    OBJECT_NAME as "{nameof(Result.RoutineName)}"
from SYS.ALL_OBJECTS
where ORACLE_MAINTAINED <> 'Y' and OBJECT_TYPE in ('FUNCTION', 'PROCEDURE')
order by OWNER, OBJECT_NAME
""";
}