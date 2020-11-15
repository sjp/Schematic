namespace SJP.Schematic.Oracle.Query
{
    internal sealed record TargetSynonymData
    {
        public string? TargetDatabaseName { get; init; }

        public string? TargetSchemaName { get; init; }

        public string? TargetObjectName { get; init; }
    }
}