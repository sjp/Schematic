﻿namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record GetRoutineCommentsQueryResult
    {
        public string ObjectType { get; init; } = default!;

        public string ObjectName { get; init; } = default!;

        public string? Comment { get; init; }
    }
}
