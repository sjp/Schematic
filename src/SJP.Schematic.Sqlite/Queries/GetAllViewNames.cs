using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetAllViewNames
{
    internal sealed record Result
    {
        public required string ViewName { get; init; }
    }

    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        return $"select name as {nameof(Result.ViewName)} from {dialect.QuoteIdentifier(schemaName)}.sqlite_master where type = 'view' order by name";
    }
}