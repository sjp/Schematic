using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetAllTableNames
{
    internal sealed record Result
    {
        public required string TableName { get; init; }
    }

    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        if (schemaName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(schemaName));

        return $"select name as {nameof(Result.TableName)} from {dialect.QuoteIdentifier(schemaName)}.sqlite_master where type = 'table' order by name";
    }
}