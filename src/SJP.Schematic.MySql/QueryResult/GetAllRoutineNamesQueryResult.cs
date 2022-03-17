namespace SJP.Schematic.MySql.QueryResult;

internal sealed record GetAllRoutineNamesQueryResult
{
    public string SchemaName { get; init; } = default!;

    public string RoutineName { get; init; } = default!;
}
