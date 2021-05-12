namespace SJP.Schematic.Sqlite.QueryResult
{
    internal sealed record GetViewNameQueryResult
    {
        public string ViewName { get; set; } = default!;
    }
}
