using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when column names across the schema mix naming conventions, such as <c>snake_case</c> and <c>PascalCase</c>.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class InconsistentColumnNamingConventionRule : Rule, ITableRule
{
    private enum NamingConvention
    {
        Ambiguous,
        SnakeCase,
        PascalOrCamelCase,
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InconsistentColumnNamingConventionRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public InconsistentColumnNamingConventionRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when column names mix naming conventions across the schema.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        var classifiedColumns = tables
            .SelectMany(static t => t.Columns.Select(c => (Table: t.Name, Column: c.Name, Convention: Classify(c.Name.LocalName))))
            .Where(static c => c.Convention != NamingConvention.Ambiguous)
            .ToList();

        var snakeCount = classifiedColumns.Count(static c => c.Convention == NamingConvention.SnakeCase);
        var pascalOrCamelCount = classifiedColumns.Count - snakeCount;

        // No inconsistency unless both conventions are present in the schema.
        if (snakeCount == 0 || pascalOrCamelCount == 0)
            return Task.FromResult<IReadOnlyCollection<IRuleMessage>>([]);

        var dominantConvention = snakeCount >= pascalOrCamelCount
            ? NamingConvention.SnakeCase
            : NamingConvention.PascalOrCamelCase;

        var messages = classifiedColumns
            .Where(c => c.Convention != dominantConvention)
            .Select(c => BuildMessage(c.Table, c.Column))
            .ToList();

        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    private static NamingConvention Classify(string columnName)
    {
        var hasUnderscore = columnName.Contains('_', StringComparison.Ordinal);
        var hasUpper = columnName.Any(char.IsUpper);

        if (hasUnderscore && !hasUpper)
            return NamingConvention.SnakeCase;
        if (hasUpper && !hasUnderscore)
            return NamingConvention.PascalOrCamelCase;

        return NamingConvention.Ambiguous;
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table containing the column.</param>
    /// <param name="columnName">The name of the column that deviates from the dominant naming convention.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="columnName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName, Identifier columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(columnName);

        var messageText = $"The column '{columnName.LocalName}' in the table {tableName} does not follow the dominant naming convention used elsewhere in the schema. Consider using a consistent convention for all column names.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0035";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Inconsistent column naming convention.";
}
