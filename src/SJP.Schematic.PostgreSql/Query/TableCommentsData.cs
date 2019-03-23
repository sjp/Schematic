namespace SJP.Schematic.PostgreSql.Query
{
    internal class TableCommentsData : CommentsData
    {
        public string TableName { get; set; }

        public string ObjectType { get; set; }
    }
}
