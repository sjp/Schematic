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
/// A linting rule which reports when a foreign key uses a <c>SET DEFAULT</c> referential action but one or more of its columns have no default value.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class ForeignKeySetDefaultReferentialActionRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKeySetDefaultReferentialActionRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public ForeignKeySetDefaultReferentialActionRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when a foreign key has a <c>SET DEFAULT</c> referential action applied to columns without a default value.
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
    /// Analyses a database table. Reports messages when a foreign key has a <c>SET DEFAULT</c> referential action applied to columns without a default value.
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
            var setsDefault = foreignKey.DeleteAction == ReferentialAction.SetDefault
                || foreignKey.UpdateAction == ReferentialAction.SetDefault;
            if (!setsDefault)
                continue;

            var hasColumnWithoutDefault = foreignKey.ChildKey.Columns.Any(static c => c.DefaultValue.IsNone);
            if (!hasColumnWithoutDefault)
                continue;

            var ruleMessage = BuildMessage(foreignKey.ChildKey.Name, foreignKey.ChildTable);
            result.Add(ruleMessage);
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
        builder.Append("The table ")
            .Append(childTableName)
            .Append(" has a foreign key");
        foreignKeyName.IfSome(name =>
        {
            builder.Append(" '")
                .Append(name.LocalName)
                .Append('\'');
        });

        builder.Append(" with a SET DEFAULT referential action, but one or more of its columns have no default value. The action can never succeed and will cause referential operations to fail.");

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0026";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Foreign key with a SET DEFAULT action on columns without a default value.";
}
