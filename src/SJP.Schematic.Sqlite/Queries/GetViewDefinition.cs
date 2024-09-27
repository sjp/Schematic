using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetViewDefinition
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string ViewName { get; init; }
    }

    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        return $"select sql from {dialect.QuoteIdentifier(schemaName)}.sqlite_master where type = 'view' and tbl_name = @{nameof(Query.ViewName)}";
    }
}