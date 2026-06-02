using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when columns that share the same name across different tables are declared with differing types.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class ColumnTypeMismatchAcrossTablesRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnTypeMismatchAcrossTablesRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public ColumnTypeMismatchAcrossTablesRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when identically named columns across tables have differing types.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        var columnsByName = tables
            .SelectMany(static t => t.Columns.Select(c => (Table: t.Name, Column: c)))
            .GroupBy(static tc => tc.Column.Name.LocalName, StringComparer.Ordinal);

        var messages = new List<IRuleMessage>();
        foreach (var columnGroup in columnsByName)
        {
            var distinctTypes = columnGroup
                .Select(static tc => tc.Column.Type.Definition)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (distinctTypes.Count <= 1)
                continue;

            var tableNames = columnGroup
                .Select(static tc => tc.Table)
                .Distinct()
                .ToList();

            messages.Add(BuildMessage(columnGroup.Key, tableNames));
        }

        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="columnName">The name of the column shared across tables.</param>
    /// <param name="tableNames">The tables that declare the column with differing types.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="columnName"/> or <paramref name="tableNames"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(string columnName, IReadOnlyCollection<Identifier> tableNames)
    {
        ArgumentNullException.ThrowIfNull(columnName);
        ArgumentNullException.ThrowIfNull(tableNames);

        var builder = StringBuilderCache.Acquire();
        builder.Append("The column '")
            .Append(columnName)
            .Append("' is declared with differing types across the following tables: ")
            .AppendJoin(", ", tableNames.Select(static t => t.ToString()))
            .Append(". Consider using a consistent type to avoid implicit conversions and join errors.");

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0028";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Identically named columns have differing types across tables.";
}
