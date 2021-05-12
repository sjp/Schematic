namespace SJP.Schematic.MySql.QueryResult
{
    internal sealed record GetAllRoutineCommentsQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;

        public string? Comment { get; init; }
    }
}
