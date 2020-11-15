namespace SJP.Schematic.Oracle.Query
{
    internal sealed record SynonymData
    {
        public string? SchemaName { get; init; }

        public string? SynonymName { get; init; }

        public string? TargetDatabaseName { get; init; }

        public string? TargetSchemaName { get; init; }

        public string? TargetObjectName { get; init; }
    }
}