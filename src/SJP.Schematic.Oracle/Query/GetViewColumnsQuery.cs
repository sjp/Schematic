﻿namespace SJP.Schematic.Oracle.Query
{
    internal sealed record GetViewColumnsQuery
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }
}
