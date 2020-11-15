namespace SJP.Schematic.Oracle.Query
{
    internal sealed record RoutineData
    {
        public string? SchemaName { get; init; }

        public string? RoutineName { get; init; }

        public string? RoutineType { get; init; }

        public int LineNumber { get; init; }

        public string? Text { get; init; }
    }
}