namespace SJP.Schematic.Oracle.QueryResult;

internal sealed record GetPackageNameQueryResult
{
    public string SchemaName { get; init; } = default!;

    public string PackageName { get; init; } = default!;
}