﻿namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record GetRoutineDefinitionQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;

        public string? Definition { get; init; }
    }
}
