﻿using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetIndexDefinition
{
    internal sealed record Query
    {
        public string TableName { get; init; } = default!;

        public string IndexName { get; init; } = default!;
    }

    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        if (schemaName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(schemaName));

        return $"select sql from {dialect.QuoteIdentifier(schemaName)}.sqlite_master where type = 'index' and tbl_name = @{nameof(Query.TableName)} and name = @{nameof(Query.IndexName)}";
    }
}