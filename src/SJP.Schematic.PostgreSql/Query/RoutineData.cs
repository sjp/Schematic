namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed record RoutineData
    {
        public string? SchemaName { get; init; }

        public string? RoutineName { get; init; }

        public string? Definition { get; init; }
    }
}
