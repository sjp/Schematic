namespace SJP.Schematic.Sqlite.Query
{
    internal sealed record GetTableNameQuery
    {
        public string TableName { get; set; } = default!;
    }
}
