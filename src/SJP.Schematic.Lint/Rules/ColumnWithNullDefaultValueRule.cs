using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when columns in a table have a default value defined with a value of null.
/// </summary>
/// <seealso cref="Rule" />
/// <seealso cref="ITableRule" />
public class ColumnWithNullDefaultValueRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnWithNullDefaultValueRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public ColumnWithNullDefaultValueRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when columns have a default value defined with a value of null.
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
    /// Analyses a database table. Reports messages when columns have a default value defined with a value of null.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var nullableColumns = table.Columns.Where(c => c.IsNullable).ToList();
        if (nullableColumns.Empty())
            return [];

        var result = new List<IRuleMessage>();

        foreach (var nullableColumn in nullableColumns)
        {
            nullableColumn.DefaultValue
                .Where(IsNullDefaultValue)
                .IfSome(_ =>
            {
                var ruleMessage = BuildMessage(table.Name, nullableColumn.Name.LocalName);
                result.Add(ruleMessage);
            });
        }

        return result;
    }

    /// <summary>
    /// Determines whether a default value definition is a null value.
    /// </summary>
    /// <param name="defaultValue">A column's default value.</param>
    /// <returns><see langword="true" /> if the provided default value is a null value; otherwise, <see langword="false" />.</returns>
    protected static bool IsNullDefaultValue(string defaultValue)
    {
        return !defaultValue.IsNullOrWhiteSpace()
            && NullValues.Contains(defaultValue, StringComparer.Ordinal);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">The name of the column whose default value is null.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var messageText = $"The table '{tableName}' has a column '{columnName}' whose default value is null. Consider removing the default value on the column.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0002";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Null default values assigned to column.";

    private static readonly IEnumerable<string> NullValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "null", "(null)" };
}