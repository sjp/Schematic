﻿namespace SJP.Schematic.MySql.Query
{
    internal sealed record GetAllTableCommentsQuery
    {
        public string SchemaName { get; init; } = default!;
    }
}
