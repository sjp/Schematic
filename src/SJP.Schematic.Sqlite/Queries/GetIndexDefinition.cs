using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetIndexDefinition
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string TableName { get; init; }

        public required string IndexName { get; init; }
    }

    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        return $"select sql from {dialect.QuoteIdentifier(schemaName)}.sqlite_master where type = 'index' and tbl_name = @{nameof(Query.TableName)} and name = @{nameof(Query.IndexName)}";
    }
}