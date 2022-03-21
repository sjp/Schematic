namespace SJP.Schematic.Oracle.QueryResult;

internal sealed record GetPackageDefinitionQueryResult
{
    public string RoutineType { get; init; } = default!;

    public int LineNumber { get; init; }

    public string? Text { get; init; }
}