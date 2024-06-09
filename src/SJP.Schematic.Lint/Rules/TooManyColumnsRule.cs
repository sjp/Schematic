using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when too many columns are present on a database table.
/// </summary>
/// <seealso cref="Rule" />
/// <seealso cref="ITableRule" />
public class TooManyColumnsRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TooManyColumnsRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    /// <param name="columnLimit">The column limit. When exceeded, this rule will report issues.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="columnLimit"/> is less than one.</exception>
    public TooManyColumnsRule(RuleLevel level, uint columnLimit = 100)
        : base(RuleId, RuleTitle, level)
    {
        if (columnLimit == 0)
            throw new ArgumentOutOfRangeException(nameof(columnLimit), "The column limit must be at least 1.");

        ColumnLimit = columnLimit;
    }

    /// <summary>
    /// Retrieves the upper column limit. When exceeded, this rule will report issues.
    /// </summary>
    /// <value>The upper column limit.</value>
    protected uint ColumnLimit { get; }

    /// <summary>
    /// Analyses database tables. Reports messages when tables contain too many columns.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <c>null</c>.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        return tables.SelectMany(AnalyseTable).ToAsyncEnumerable();
    }

    /// <summary>
    /// Analyses a database table. Reports messages when a table contains too many columns.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
    protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var columnCount = table.Columns.Count;
        if (columnCount <= ColumnLimit)
            return [];

        var message = BuildMessage(table.Name, columnCount);
        return [message];
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnCount">The number of columns in the table.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName, int columnCount)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageText = $"The table {tableName} has too many columns. It has {columnCount.ToString(CultureInfo.InvariantCulture)} columns.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0021";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Too many columns present on the table.";
}