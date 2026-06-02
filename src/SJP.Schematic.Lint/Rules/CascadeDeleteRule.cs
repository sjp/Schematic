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
/// A linting rule which reports when a foreign key is configured with a <c>CASCADE</c> delete action. Such relationships can lead to large, unintended deletions.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class CascadeDeleteRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CascadeDeleteRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public CascadeDeleteRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when a foreign key uses a <c>CASCADE</c> delete action.
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
    /// Analyses a database table. Reports messages when a foreign key uses a <c>CASCADE</c> delete action.
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
            if (foreignKey.DeleteAction != ReferentialAction.Cascade)
                continue;

            result.Add(BuildMessage(foreignKey.ChildKey.Name, foreignKey.ChildTable, foreignKey.ParentTable));
        }

        return result;
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
            .Append(" has a CASCADE delete action. Deleting a parent row will also delete the related child rows; ensure this is intended, as cascades can propagate and remove large amounts of data.");

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0031";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Foreign key with a CASCADE delete action.";
}
