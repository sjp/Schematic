﻿using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Query
{
    internal sealed record GetViewDefinitionQuery
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }
}
