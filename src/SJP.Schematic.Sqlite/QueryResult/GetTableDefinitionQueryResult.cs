namespace SJP.Schematic.Sqlite.QueryResult
{
    internal sealed record GetTableDefinitionQueryResult
    {
        public string Definition { get; set; } = default!;
    }
}
