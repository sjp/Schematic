using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetViewName
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string ViewName { get; init; }
    }

    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        if (schemaName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(schemaName));

        return $"select name from {dialect.QuoteIdentifier(schemaName)}.sqlite_master where type = 'view' and lower(name) = lower(@{nameof(Query.ViewName)})";
    }
}