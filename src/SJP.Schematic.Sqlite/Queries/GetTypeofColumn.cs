using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetTypeofColumn
{
    internal static string Sql(IDatabaseDialect dialect, Identifier tableName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        return $"select typeof({dialect.QuoteName(columnName)}) from {dialect.QuoteName(tableName)} limit 1";
    }
}