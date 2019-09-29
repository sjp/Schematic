namespace SJP.Schematic.PostgreSql.Query
{
    internal class ViewCommentsData : CommentsData
    {
        public string? ViewName { get; set; }

        public string? ObjectType { get; set; }
    }
}
