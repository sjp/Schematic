using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetTypeofColumn
{
    internal static string Sql(IDatabaseDialect dialect, Identifier tableName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentNullException.ThrowIfNull(tableName);
        if (columnName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(columnName));

        return $"select typeof({dialect.QuoteName(columnName)}) from {dialect.QuoteName(tableName)} limit 1";
    }
}