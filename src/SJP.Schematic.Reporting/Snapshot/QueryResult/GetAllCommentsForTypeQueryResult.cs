namespace SJP.Schematic.Reporting.Snapshot.QueryResult
{
    internal record GetAllCommentsForTypeQueryResult
    {
        public string? ServerName { get; init; }

        public string? DatabaseName { get; init; }

        public string? SchemaName { get; init; }

        public string LocalName { get; init; } = default!;
    }
}
