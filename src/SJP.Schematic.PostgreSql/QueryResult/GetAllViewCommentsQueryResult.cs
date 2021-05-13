namespace SJP.Schematic.PostgreSql.QueryResult
{
    internal sealed record GetAllViewCommentsQueryResult
    {
        public string? SchemaName { get; init; }

        public string? ViewName { get; init; }

        public string? ObjectType { get; init; }

        public string? ObjectName { get; init; }

        public string? Comment { get; init; }
    }
}
