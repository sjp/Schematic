namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record GetAllViewCommentsQuery
    {
        public string CommentProperty { get; init; } = default!;
    }
}
