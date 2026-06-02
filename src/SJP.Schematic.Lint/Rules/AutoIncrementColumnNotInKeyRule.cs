using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when an auto-incrementing column does not participate in any candidate key.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class AutoIncrementColumnNotInKeyRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoIncrementColumnNotInKeyRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public AutoIncrementColumnNotInKeyRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when an auto-incrementing column is not part of any candidate key.
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
    /// Analyses a database table. Reports messages when an auto-incrementing column is not part of any candidate key.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var keyColumnNames = table.PrimaryKey
            .Match(static pk => pk.Columns, static () => [])
            .Concat(table.UniqueKeys.SelectMany(static uk => uk.Columns))
            .Select(static c => c.Name.LocalName)
            .ToHashSet(StringComparer.Ordinal);

        var result = new List<IRuleMessage>();
        foreach (var column in table.Columns)
        {
            if (column.AutoIncrement.IsNone)
                continue;
            if (keyColumnNames.Contains(column.Name.LocalName))
                continue;

            result.Add(BuildMessage(table.Name, column.Name));
        }

        return result;
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">The name of the auto-incrementing column.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="columnName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName, Identifier columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(columnName);

        var messageText = $"The table {tableName} has an auto-incrementing column '{columnName.LocalName}' that is not part of any primary or unique key. This is often unintended.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0029";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Auto-incrementing column is not part of any candidate key.";
}
