﻿namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed record GetV10TableColumnsQuery
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }
}
