namespace SJP.Schematic.SqlServer.Query
{
    internal class TableCommentsData
    {
        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public string ObjectType { get; set; }

        public string ObjectName { get; set; }

        public string Comment { get; set; }
    }
}
