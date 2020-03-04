namespace SJP.Schematic.SqlServer.Query
{
    internal class CommentsData
    {
        public string SchemaName { get; set; } = default!;

        public string TableName { get; set; } = default!;

        public string ObjectType { get; set; } = default!;

        public string ObjectName { get; set; } = default!;

        public string? Comment { get; set; }
    }
}
