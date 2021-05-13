namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetUserSynonymDefinitionQueryResult
    {
        public string? TargetDatabaseName { get; init; }

        public string? TargetSchemaName { get; init; }

        public string? TargetObjectName { get; init; }
    }
}
