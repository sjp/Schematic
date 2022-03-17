namespace SJP.Schematic.MySql.Query;

internal sealed record GetRoutineDefinitionQuery
{
    public string SchemaName { get; init; } = default!;

    public string RoutineName { get; init; } = default!;
}
