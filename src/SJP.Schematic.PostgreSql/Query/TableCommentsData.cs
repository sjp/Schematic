namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed class TableCommentsData : CommentsData
    {
        public string? TableName { get; set; }

        public string? ObjectType { get; set; }
    }
}
