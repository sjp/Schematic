namespace SJP.Schematic.Oracle.Queries;

internal static class GetDatabaseVersion
{
    internal sealed record Result
    {
        public required string? ProductName { get; init; }

        public required string? VersionNumber { get; init; }
    }

    internal const string Sql = $"""

select
    PRODUCT as "{nameof(Result.ProductName)}",
    VERSION as "{nameof(Result.VersionNumber)}"
from PRODUCT_COMPONENT_VERSION
where PRODUCT like 'Oracle Database%'
""";
}