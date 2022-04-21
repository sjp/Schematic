using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetTypeofColumn
{
    internal static string Sql(IDatabaseDialect dialect, Identifier tableName, string columnName)
    {
        if (dialect == null)
            throw new ArgumentNullException(nameof(dialect));
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (columnName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(columnName));

        return $"select typeof({ dialect.QuoteName(columnName) }) from { dialect.QuoteName(tableName) } limit 1";
    }
}