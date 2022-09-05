using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when a table contains no non-nullable columns.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class NoNonNullableColumnsPresentRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoNonNullableColumnsPresentRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public NoNonNullableColumnsPresentRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when no non-nullable columns are present on tables.
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
    /// Analyses a database table. Reports messages when no non-nullable columns are present on a table.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
    protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var containsNotNullColumns = table.Columns.Any(c => !c.IsNullable);
        if (containsNotNullColumns)
            return Array.Empty<IRuleMessage>();

        var message = BuildMessage(table.Name);
        return new[] { message };
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageText = $"The table '{tableName}' has no not-nullable columns present. Consider adding one to ensure that each record contains data.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0012";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "No not-null columns present on the table.";
}