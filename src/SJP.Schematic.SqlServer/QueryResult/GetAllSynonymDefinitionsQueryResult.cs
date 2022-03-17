﻿namespace SJP.Schematic.SqlServer.QueryResult;

internal sealed record GetAllSynonymDefinitionsQueryResult
{
    public string SchemaName { get; init; } = default!;

    public string SynonymName { get; init; } = default!;

    public string? TargetServerName { get; init; }

    public string? TargetDatabaseName { get; init; }

    public string? TargetSchemaName { get; init; }

    public string TargetObjectName { get; init; } = default!;
}
