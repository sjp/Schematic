namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record GetAllTableCommentsQuery
    {
        public string CommentProperty { get; init; } = default!;
    }
}
