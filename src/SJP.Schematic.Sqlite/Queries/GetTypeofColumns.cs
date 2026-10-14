using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetTypeofColumns
{
    /// <summary>
    /// Separates the runtime type names in the single value returned by the query. <c>typeof()</c> only
    /// returns <c>null</c>, <c>integer</c>, <c>real</c>, <c>text</c> or <c>blob</c>, so it never occurs within a type name.
    /// </summary>
    internal const char TypeNameSeparator = ',';

    // the concatenation is a left-deep expression tree, so its depth grows with every column;
    // parenthesised groups keep it well below SQLite's default expression depth limit of 1000
    private const int ColumnsPerGroup = 100;

    internal static string Sql(IDatabaseDialect dialect, Identifier viewName, IReadOnlyCollection<string> columnNames)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentNullException.ThrowIfNull(viewName);
        ArgumentNullException.ThrowIfNull(columnNames);
        if (columnNames.Empty())
            throw new ArgumentException("At least one column name must be provided.", nameof(columnNames));

        var quotedViewName = dialect.QuoteName(viewName);
        var quotedRowAlias = dialect.QuoteIdentifier("__schematic_row");

        // The view's first row is read once, and the runtime type of each requested column is joined
        // into a single value, so the result is one row with one value regardless of how many columns
        // are requested. When the view returns no rows, neither does this query.
        var separator = $" || '{TypeNameSeparator}' || ";
        var groups = columnNames
            .Select(columnName => $"typeof({dialect.QuoteName(columnName)})")
            .Chunk(ColumnsPerGroup)
            .Select(group => "(" + group.Join(separator) + ")")
            .Join(separator);

        return $"with {quotedRowAlias} as (select * from {quotedViewName} limit 1){Environment.NewLine}select {groups}{Environment.NewLine}from {quotedRowAlias}";
    }
}
