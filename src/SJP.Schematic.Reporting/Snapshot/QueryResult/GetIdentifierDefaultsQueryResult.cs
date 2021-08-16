namespace SJP.Schematic.Reporting.Snapshot.QueryResult
{
    internal record GetIdentifierDefaultsQueryResult
    {
        public string? Server { get; init; }

        public string? Database { get; init; }

        public string? Schema { get; init; }
    }
}
