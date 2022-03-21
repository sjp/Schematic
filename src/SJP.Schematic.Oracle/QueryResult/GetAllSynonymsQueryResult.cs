namespace SJP.Schematic.Oracle.QueryResult;

internal sealed record GetAllSynonymsQueryResult
{
    public string? SchemaName { get; init; }

    public string? SynonymName { get; init; }

    public string? TargetDatabaseName { get; init; }

    public string? TargetSchemaName { get; init; }

    public string? TargetObjectName { get; init; }
}