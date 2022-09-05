namespace SJP.Schematic.Oracle.Queries;

internal static class GetAllPackageNames
{
    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string PackageName { get; init; } = default!;
    }

    internal const string Sql = @$"
SELECT
    OWNER as ""{nameof(Result.SchemaName)}"",
    OBJECT_NAME as ""{nameof(Result.PackageName)}""
FROM SYS.ALL_OBJECTS
WHERE ORACLE_MAINTAINED <> 'Y' AND OBJECT_TYPE = 'PACKAGE'
ORDER BY OWNER, OBJECT_NAME";
}