namespace SJP.Schematic.PostgreSql.QueryResult
{
    internal sealed record GetAllTableCommentsQueryResult
    {
        public string? SchemaName { get; init; }

        public string? TableName { get; init; }

        public string? ObjectType { get; init; }

        public string? ObjectName { get; init; }

        public string? Comment { get; init; }
    }
}
