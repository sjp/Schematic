namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record GetAllSequenceCommentsQuery
    {
        public string CommentProperty { get; init; } = default!;
    }
}
