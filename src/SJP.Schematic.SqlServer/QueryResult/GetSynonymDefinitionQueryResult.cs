namespace SJP.Schematic.SqlServer.QueryResult;

internal sealed record GetSynonymDefinitionQueryResult
{
    public string? TargetServerName { get; init; }

    public string? TargetDatabaseName { get; init; }

    public string? TargetSchemaName { get; init; }

    public string TargetObjectName { get; init; } = default!;
}
