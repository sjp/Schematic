namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record GetAllSynonymCommentsQuery
    {
        public string CommentProperty { get; init; } = default!;
    }
}
