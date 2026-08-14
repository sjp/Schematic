using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetTypeofColumns
{
    internal sealed record Result
    {
        public required string ColumnName { get; init; }

        public required string? TypeName { get; init; }
    }

    internal static string Sql(IDatabaseDialect dialect, Identifier viewName, IReadOnlyCollection<string> columnNames)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentNullException.ThrowIfNull(viewName);
        ArgumentNullException.ThrowIfNull(columnNames);
        if (columnNames.Empty())
            throw new ArgumentException("At least one column name must be provided.", nameof(columnNames));

        var quotedViewName = dialect.QuoteName(viewName);

        // A single query is used to reduce this to one round-trip instead of one per column.
        // Each column's type is read via its own scalar subquery, joined together with `union all`,
        // rather than selecting every `typeof()` expression in one row, so that the result shape
        // stays a fixed two-column (name, type) record regardless of how many columns are requested.
        // SQLite doesn't allow parenthesising a branch of a compound select (unlike Postgres/MySQL),
        // so `limit 1` is instead scoped per-column via a scalar subquery in the select list.
        return columnNames
            .Select(columnName =>
            {
                var quotedColumnName = dialect.QuoteName(columnName);
                var literalColumnName = columnName.Replace("'", "''", StringComparison.Ordinal);
                return $"select '{literalColumnName}' as \"{nameof(Result.ColumnName)}\", (select typeof({quotedColumnName}) from {quotedViewName} limit 1) as \"{nameof(Result.TypeName)}\"";
            })
            .Join(Environment.NewLine + "union all" + Environment.NewLine);
    }
}
