namespace SJP.Schematic.Reporting.Snapshot.Query
{
    internal record AddDatabaseObjectQuery
    {
        public string ObjectType { get; init; } = default!;

        public string? ServerName { get; init; }

        public string? DatabaseName { get; init; }

        public string? SchemaName { get; init; }

        public string? LocalName { get; init; }

        public string DefinitionJson { get; init; } = default!;
    }
}
