namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record GetViewNameQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }
}
