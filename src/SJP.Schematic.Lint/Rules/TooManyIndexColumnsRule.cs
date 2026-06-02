using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when an index spans more columns than a configured limit. Very wide indexes are rarely useful and carry a high maintenance cost.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class TooManyIndexColumnsRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TooManyIndexColumnsRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    /// <param name="columnLimit">The maximum number of columns an index may contain before being reported.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="columnLimit"/> is zero.</exception>
    public TooManyIndexColumnsRule(RuleLevel level, uint columnLimit = 5)
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
    /// Analyses database tables. Reports messages when an index contains too many columns.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        var messages = tables.SelectMany(AnalyseTable).ToList();
        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    /// <summary>
    /// Analyses a database table. Reports messages when an index contains too many columns.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var result = new List<IRuleMessage>();
        foreach (var index in table.Indexes)
        {
            if (index.Columns.Count <= ColumnLimit)
                continue;

            result.Add(BuildMessage(table.Name, index.Name, index.Columns.Count));
        }

        return result;
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="indexName">The name of the index.</param>
    /// <param name="columnCount">The number of columns the index contains.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="indexName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName, Identifier indexName, int columnCount)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(indexName);

        var messageText = $"The table {tableName} has an index '{indexName.LocalName}' with {columnCount.ToString()} columns, which exceeds the configured limit of {ColumnLimit.ToString()}. Consider whether such a wide index is necessary.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0033";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Index contains too many columns.";
}
