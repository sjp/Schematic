namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record CommentsData
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;

        public string ObjectType { get; init; } = default!;

        public string ObjectName { get; init; } = default!;

        public string? Comment { get; init; }
    }
}
