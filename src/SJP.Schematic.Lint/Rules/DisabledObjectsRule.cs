using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when disabled objects are present on tables in a database.
/// </summary>
/// <seealso cref="Rule" />
/// <seealso cref="ITableRule" />
public class DisabledObjectsRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DisabledObjectsRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public DisabledObjectsRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when disabled database objects are discovered.
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
    /// Analyses a database table. Reports messages when disabled database objects are discovered.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var result = new List<IRuleMessage>();

        var disabledForeignKeys = table.ParentKeys
            .Select(fk => fk.ChildKey)
            .Where(fk => !fk.IsEnabled);
        foreach (var foreignKey in disabledForeignKeys)
        {
            var ruleMessage = BuildDisabledForeignKeyMessage(table.Name, foreignKey.Name);
            result.Add(ruleMessage);
        }

        var primaryKey = table.PrimaryKey;
        primaryKey
            .Where(pk => !pk.IsEnabled)
            .Map(pk => BuildDisabledPrimaryKeyMessage(table.Name, pk.Name))
            .IfSome(result.Add);

        var disabledUniqueKeys = table.UniqueKeys.Where(uk => !uk.IsEnabled);
        foreach (var uniqueKey in disabledUniqueKeys)
        {
            var ruleMessage = BuildDisabledUniqueKeyMessage(table.Name, uniqueKey.Name);
            result.Add(ruleMessage);
        }

        var disabledChecks = table.Checks.Where(ck => !ck.IsEnabled);
        foreach (var check in disabledChecks)
        {
            var ruleMessage = BuildDisabledCheckConstraintMessage(table.Name, check.Name);
            result.Add(ruleMessage);
        }

        var disabledIndexes = table.Indexes.Where(ix => !ix.IsEnabled);
        foreach (var index in disabledIndexes)
        {
            var ruleMessage = BuildDisabledIndexMessage(table.Name, index.Name?.LocalName);
            result.Add(ruleMessage);
        }

        var disabledTriggers = table.Triggers.Where(uk => !uk.IsEnabled);
        foreach (var trigger in disabledTriggers)
        {
            var ruleMessage = BuildDisabledTriggerMessage(table.Name, trigger.Name?.LocalName);
            result.Add(ruleMessage);
        }

        return result;
    }

    /// <summary>
    /// Builds a message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="foreignKeyName">The name of the foreign key, if available.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildDisabledForeignKeyMessage(Identifier tableName, Option<Identifier> foreignKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageKeyName = foreignKeyName.Match(
            name => " '" + name.LocalName + "'",
            () => string.Empty
        );

        var messageText = $"The table '{tableName}' contains a disabled foreign key{messageKeyName}. Consider enabling or removing the foreign key.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// Builds a message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="primaryKeyName">The name of the primary key, if available.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildDisabledPrimaryKeyMessage(Identifier tableName, Option<Identifier> primaryKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageKeyName = primaryKeyName.Match(
            name => " '" + name.LocalName + "'",
            () => string.Empty
        );

        var messageText = $"The table '{tableName}' contains a disabled primary key{messageKeyName}. Consider enabling or removing the primary key.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// Builds a message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="uniqueKeyName">The name of the unique key, if available.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildDisabledUniqueKeyMessage(Identifier tableName, Option<Identifier> uniqueKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageKeyName = uniqueKeyName.Match(
            name => " '" + name.LocalName + "'",
            () => string.Empty
        );

        var messageText = $"The table '{tableName}' contains a disabled unique key{messageKeyName}. Consider enabling or removing the unique key.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// Builds a message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="checkName">The name of the check constraint, if available.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildDisabledCheckConstraintMessage(Identifier tableName, Option<Identifier> checkName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageCheckName = checkName.Match(
            name => " '" + name.LocalName + "'",
            () => string.Empty
        );

        var messageText = $"The table '{tableName}' contains a disabled check constraint{messageCheckName}. Consider enabling or removing the check constraint.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// Builds a message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="indexName">The name of the index, if available.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildDisabledIndexMessage(Identifier tableName, string? indexName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageIndexName = !indexName.IsNullOrWhiteSpace()
            ? " '" + indexName + "'"
            : string.Empty;

        var messageText = $"The table '{tableName}' contains a disabled index{messageIndexName}. Consider enabling or removing the index.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// Builds a message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="triggerName">The name of the trigger, if available.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildDisabledTriggerMessage(Identifier tableName, string? triggerName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageTriggerName = !triggerName.IsNullOrWhiteSpace()
            ? " '" + triggerName + "'"
            : string.Empty;

        var messageText = $"The table '{tableName}' contains a disabled trigger{messageTriggerName}. Consider enabling or removing the trigger.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0004";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Disabled constraint, index or triggers present on a table.";
}