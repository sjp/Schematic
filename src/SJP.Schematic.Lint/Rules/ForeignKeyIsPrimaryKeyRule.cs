using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when a foreign key is also a primary key for a table.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class ForeignKeyIsPrimaryKeyRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKeyIsPrimaryKeyRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public ForeignKeyIsPrimaryKeyRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when a foreign key is also the primary key for a table.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <c>null</c>.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        if (tables == null)
            throw new ArgumentNullException(nameof(tables));

        return tables.SelectMany(AnalyseTable).ToAsyncEnumerable();
    }

    /// <summary>
    /// Analyses a database table. Reports messages when a foreign key is also the primary key for a table.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
    protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        if (table == null)
            throw new ArgumentNullException(nameof(table));

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
    /// <exception cref="ArgumentNullException"><paramref name="childTableName"/> is <c>null</c>.</exception>
    protected virtual IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier childTableName)
    {
        if (childTableName == null)
            throw new ArgumentNullException(nameof(childTableName));

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
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0007";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Foreign key relationships contains the same columns as the target key.";
}