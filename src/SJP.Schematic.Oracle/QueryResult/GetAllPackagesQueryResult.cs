namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetAllPackagesQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string PackageName { get; init; } = default!;

        public string RoutineType { get; init; } = default!;

        public int LineNumber { get; init; }

        public string? Text { get; init; }
    }
}
