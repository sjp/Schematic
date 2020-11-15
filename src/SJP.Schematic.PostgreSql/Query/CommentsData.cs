namespace SJP.Schematic.PostgreSql.Query
{
    internal record CommentsData
    {
        public string? SchemaName { get; init; }

        public string? ObjectName { get; init; }

        public string? Comment { get; init; }
    }
}
