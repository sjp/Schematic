namespace SJP.Schematic.Reporting.Snapshot.Query
{
    internal record GetDatabaseObjectDefinitionQuery
    {
        public string ObjectType { get; init; } = default!;

        public string? DatabaseName { get; init; }

        public string? SchemaName { get; init; }

        public string LocalName { get; init; } = default!;
    }
}
