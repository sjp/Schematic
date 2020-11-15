namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record SynonymData
    {
        public string SchemaName { get; init; } = default!;

        public string ObjectName { get; init; } = default!;

        public string? TargetServerName { get; init; }

        public string? TargetDatabaseName { get; init; }

        public string? TargetSchemaName { get; init; }

        public string TargetObjectName { get; init; } = default!;
    }
}