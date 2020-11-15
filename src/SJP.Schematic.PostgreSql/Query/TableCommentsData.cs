namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed record TableCommentsData : CommentsData
    {
        public string? TableName { get; init; }

        public string? ObjectType { get; init; }
    }
}
