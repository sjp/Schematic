﻿namespace SJP.Schematic.SqlServer.Query;

internal sealed record GetMaterializedViewDefinitionQuery
{
    public string SchemaName { get; init; } = default!;

    public string ViewName { get; init; } = default!;
}