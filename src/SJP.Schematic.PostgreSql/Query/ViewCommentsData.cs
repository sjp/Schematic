namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed record ViewCommentsData : CommentsData
    {
        public string? ViewName { get; init; }

        public string? ObjectType { get; init; }
    }
}
