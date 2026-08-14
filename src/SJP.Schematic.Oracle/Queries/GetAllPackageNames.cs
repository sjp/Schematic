namespace SJP.Schematic.Oracle.Queries;

internal static class GetAllPackageNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string PackageName { get; init; }
    }

    internal const string Sql = $"""

select
    OWNER as "{nameof(Result.SchemaName)}",
    OBJECT_NAME as "{nameof(Result.PackageName)}"
from SYS.ALL_OBJECTS
where ORACLE_MAINTAINED <> 'Y' and OBJECT_TYPE = 'PACKAGE'
order by OWNER, OBJECT_NAME
""";
}