using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetTableDefinition
{
    internal sealed record Query
    {
        public string TableName { get; set; } = default!;
    }

    internal sealed record Result
    {
        public string Definition { get; set; } = default!;
    }

    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        if (schemaName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(schemaName));

        return $"select sql from { dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'table' and tbl_name = @{ nameof(Query.TableName) }";
    }
}