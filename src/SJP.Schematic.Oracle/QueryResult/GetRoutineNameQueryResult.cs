namespace SJP.Schematic.Oracle.QueryResult;

internal sealed record GetRoutineNameQueryResult
{
    public string SchemaName { get; init; } = default!;

    public string RoutineName { get; init; } = default!;
}