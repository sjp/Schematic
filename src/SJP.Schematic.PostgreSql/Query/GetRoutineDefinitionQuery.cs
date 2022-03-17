﻿namespace SJP.Schematic.PostgreSql.Query;

internal sealed record GetRoutineDefinitionQuery
{
    public string SchemaName { get; init; } = default!;

    public string RoutineName { get; init; } = default!;
}
