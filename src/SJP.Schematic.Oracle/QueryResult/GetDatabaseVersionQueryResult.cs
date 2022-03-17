namespace SJP.Schematic.Oracle.QueryResult;

internal sealed record GetDatabaseVersionQueryResult
{
    public string? ProductName { get; init; }

    public string? VersionNumber { get; init; }
}
