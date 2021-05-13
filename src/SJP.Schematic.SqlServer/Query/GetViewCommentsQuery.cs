namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record GetViewCommentsQuery
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;

        public string CommentProperty { get; init; } = default!;
    }
}
