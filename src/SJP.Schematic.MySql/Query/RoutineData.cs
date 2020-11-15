namespace SJP.Schematic.MySql.Query
{
    internal sealed record RoutineData
    {
        public string SchemaName { get; init; } = default!;

        public string ObjectName { get; init; } = default!;

        public string? Definition { get; init; }
    }
}
