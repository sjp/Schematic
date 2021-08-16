namespace SJP.Schematic.Reporting.Snapshot.QueryResult
{
    internal record GetDatabaseObjectDefinitionQueryResult
    {
        public string? ServerName { get; init; }

        public string? DatabaseName { get; init; }

        public string? SchemaName { get; init; }

        public string LocalName { get; init; } = default!;

        public string? DefinitionJson { get; init; }
    }
}
