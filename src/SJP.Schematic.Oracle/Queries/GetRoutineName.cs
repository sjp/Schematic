namespace SJP.Schematic.Oracle.Queries;

internal static class GetRoutineName
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    OWNER as ""{ nameof(Result.SchemaName) }"",
    OBJECT_NAME as ""{ nameof(Result.RoutineName) }""
from SYS.ALL_OBJECTS
where OWNER = :{ nameof(Query.SchemaName) } and OBJECT_NAME = :{ nameof(Query.RoutineName) }
    and ORACLE_MAINTAINED <> 'Y' and OBJECT_TYPE in ('FUNCTION', 'PROCEDURE')";
}