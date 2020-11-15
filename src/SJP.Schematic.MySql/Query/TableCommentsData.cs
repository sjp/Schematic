namespace SJP.Schematic.MySql.Query
{
    internal sealed record TableCommentsData : CommentsData
    {
        public string TableName { get; init; } = default!;

        public string ObjectType { get; init; } = default!;
    }
}
