namespace SJP.Schematic.Oracle.Query
{
    internal sealed record TableCommentsData : CommentsData
    {
        public string? ColumnName { get; init; }

        public string? ObjectType { get; init; }
    }
}
