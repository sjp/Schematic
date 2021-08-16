namespace SJP.Schematic.Reporting.Snapshot.Query
{
    internal record AddDatabaseDefaultsQuery
    {
        public string? ServerName { get; init; }

        public string? DatabaseName { get; init; }

        public string? SchemaName { get; init; }
    }
}
