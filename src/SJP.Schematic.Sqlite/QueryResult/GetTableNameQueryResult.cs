namespace SJP.Schematic.Sqlite.QueryResult
{
    internal sealed record GetTableNameQueryResult
    {
        public string TableName { get; set; } = default!;
    }
}
