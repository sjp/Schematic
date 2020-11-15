namespace SJP.Schematic.MySql.Query
{
    internal record CommentsData
    {
        public string SchemaName { get; init; } = default!;

        public string ObjectName { get; init; } = default!;

        public string? Comment { get; init; }
    }
}
