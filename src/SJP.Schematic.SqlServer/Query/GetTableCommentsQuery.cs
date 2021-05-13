namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record GetTableCommentsQuery
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;

        public string CommentProperty { get; init; } = default!;
    }
}
