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
        var quotedRowAlias = dialect.QuoteIdentifier("__schematic_row");

        // A single query is used to reduce this to one round-trip instead of one per column.
        // The view's first row is pulled once into a CTE, which SQLite 3.35+ materializes because
        // it's referenced more than once, so the view is evaluated a single time regardless of how
        // many columns are requested. Each column's type is then read from that materialized row via
        // its own scalar subquery, joined together with `union all`, so that the result shape stays a
        // fixed two-column (name, type) record. On SQLite older than 3.35 the CTE may be inlined back
        // into each branch, which is no worse than evaluating the view per column.
        var branches = columnNames
            .Select(columnName =>
            {
                var quotedColumnName = dialect.QuoteName(columnName);
                var literalColumnName = columnName.Replace("'", "''", StringComparison.Ordinal);
                return $"select '{literalColumnName}' as \"{nameof(Result.ColumnName)}\", (select typeof({quotedColumnName}) from {quotedRowAlias}) as \"{nameof(Result.TypeName)}\"";
            })
            .Join(Environment.NewLine + "union all" + Environment.NewLine);

        return $"with {quotedRowAlias} as (select * from {quotedViewName} limit 1){Environment.NewLine}{branches}";
    }
}
