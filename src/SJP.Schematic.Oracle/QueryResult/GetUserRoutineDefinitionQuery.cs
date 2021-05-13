namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetUserRoutineDefinitionQuery
    {
        public string RoutineName { get; init; } = default!;
    }
}
