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
/// A linting rule which reports when a self-referencing foreign key constrains exactly the same set of
/// columns as the key it targets. Such a relationship can only ever match a row to itself, so it enforces nothing.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class ForeignKeyIsPrimaryKeyRule : Rule, ITableRule
{
    /// <summary>
    /// The reporting level this rule uses unless a caller overrides it: warning, because
    /// a self-referencing key over the target key's own columns is almost always a mistake.
    /// </summary>
    public const RuleLevel DefaultLevel = RuleLevel.Warning;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKeyIsPrimaryKeyRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level, or <see langword="null" /> to use <see cref="DefaultLevel"/>.</param>
    public ForeignKeyIsPrimaryKeyRule(RuleLevel? level = null)
        : base(RuleId, RuleTitle, level ?? DefaultLevel)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when a self-referencing foreign key contains the same set of columns as the key it targets.
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
    /// Analyses a database table. Reports messages when a self-referencing foreign key contains the same set of columns as the key it targets.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var result = new List<IRuleMessage>();

        var foreignKeys = table.ParentKeys;
        foreach (var foreignKey in foreignKeys)
        {
            var childTableName = foreignKey.ChildTable;
            var parentTableName = foreignKey.ParentTable;
            if (childTableName != parentTableName)
                continue;

            var childColumns = foreignKey.ChildKey.Columns;
            var parentColumns = foreignKey.ParentKey.Columns;

            var childColumnNames = childColumns.Select(c => c.Name).ToList();
            var parentColumnNames = parentColumns.Select(c => c.Name).ToList();

            var columnsEqual = childColumnNames.SequenceEqual(parentColumnNames);
            if (!columnsEqual)
                continue;

            var message = BuildMessage(foreignKey.ChildKey.Name, childTableName);
            result.Add(message);
        }

        return result;
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="foreignKeyName">The name of the foreign key constraint, if available.</param>
    /// <param name="childTableName">The name of the child table.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="childTableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier childTableName)
    {
        ArgumentNullException.ThrowIfNull(childTableName);

        var builder = StringBuilderCache.Acquire();
        builder.Append("A foreign key");
        foreignKeyName.IfSome(name =>
        {
            builder.Append(" '")
                .Append(name.LocalName)
                .Append('\'');
        });

        builder.Append(" on ")
            .Append(childTableName)
            .Append(" contains the same column set as the target key.");

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, childTableName);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId => "SCHEMATIC0007";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle => "Foreign key contains the same columns as the target key.";
}