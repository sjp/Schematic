namespace SJP.Schematic.Oracle.QueryResult;

internal sealed record GetUserPackageDefinitionQueryResult
{
    public string RoutineType { get; init; } = default!;

    public int LineNumber { get; init; }

    public string? Text { get; init; }
}
