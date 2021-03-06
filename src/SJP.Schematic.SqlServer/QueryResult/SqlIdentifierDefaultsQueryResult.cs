﻿using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record SqlIdentifierDefaultsQueryResult : IIdentifierDefaults
    {
        public string Server { get; init; } = default!;

        public string Database { get; init; } = default!;

        public string Schema { get; init; } = default!;
    }
}
