using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetViewDefinition
{
    internal sealed record Query
    {
        public string ViewName { get; set; } = default!;
    }

    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        if (dialect == null)
            throw new ArgumentNullException(nameof(dialect));
        if (schemaName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(schemaName));

        return $"select sql from { dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'view' and tbl_name = @{ nameof(Query.ViewName) }";
    }
}