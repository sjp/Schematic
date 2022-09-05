using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetTableName
{
    internal sealed record Query
    {
        public string TableName { get; set; } = default!;
    }

    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        if (schemaName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(schemaName));

        return $"select name from {dialect.QuoteIdentifier(schemaName)}.sqlite_master where type = 'table' and lower(name) = lower(@{nameof(Query.TableName)})";
    }
}