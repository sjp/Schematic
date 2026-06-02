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
/// A linting rule which reports when foreign key relationships have mismatching collations between their parent and child key columns.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class ForeignKeyColumnCollationMismatchRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKeyColumnCollationMismatchRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public ForeignKeyColumnCollationMismatchRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when foreign key relationships have mismatching collations for their parent and child key columns.
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
    /// Analyses a database table. Reports messages when a table has foreign key relationships with mismatching collations for their parent and child key columns.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var result = new List<IRuleMessage>();

        foreach (var foreignKey in table.ParentKeys)
        {
            var allCollationsMatch = foreignKey.ChildKey.Columns
                .Zip(foreignKey.ParentKey.Columns, static (child, parent) => CollationsEqual(child, parent))
                .All(static matched => matched);
            if (allCollationsMatch)
                continue;

            result.Add(BuildMessage(foreignKey.ChildKey.Name, foreignKey.ChildTable, foreignKey.ParentTable));
        }

        return result;
    }

    private static bool CollationsEqual(IDatabaseColumn childColumn, IDatabaseColumn parentColumn)
    {
        var childCollation = childColumn.Type.Collation.Match(static c => c.ToString(), static () => (string?)null);
        var parentCollation = parentColumn.Type.Collation.Match(static c => c.ToString(), static () => (string?)null);

        return string.Equals(childCollation, parentCollation, StringComparison.Ordinal);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="foreignKeyName">The name of the foreign key constraint, if available.</param>
    /// <param name="childTableName">The name of the child table.</param>
    /// <param name="parentTableName">The name of the parent table.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="childTableName"/> or <paramref name="parentTableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier childTableName, Identifier parentTableName)
    {
        ArgumentNullException.ThrowIfNull(childTableName);
        ArgumentNullException.ThrowIfNull(parentTableName);

        var builder = StringBuilderCache.Acquire();
        builder.Append("A foreign key");
        foreignKeyName.IfSome(name =>
        {
            builder.Append(" '")
                .Append(name.LocalName)
                .Append('\'');
        });

        builder.Append(" from ")
            .Append(childTableName)
            .Append(" to ")
            .Append(parentTableName)
            .Append(" contains columns with mismatching collations. These should match to avoid implicit conversions and to ensure that joins and comparisons behave consistently.");

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0030";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Foreign key relationships contain mismatching collations.";
}
