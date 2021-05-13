namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetAllRoutinesQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;

        public string RoutineType { get; init; } = default!;

        public int LineNumber { get; init; }

        public string? Text { get; init; }
    }
}
