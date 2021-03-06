﻿namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetAllViewNamesQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }
}
