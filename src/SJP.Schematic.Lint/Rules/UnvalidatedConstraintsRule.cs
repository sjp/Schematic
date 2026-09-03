using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports enabled constraints whose existing rows the database has never
/// verified, e.g. a SQL Server foreign key left untrusted by <c>WITH NOCHECK</c>, or a PostgreSQL
/// constraint left <c>NOT VALID</c>. Such a constraint is enforced for new rows only, and the query
/// planner cannot rely upon it.
/// </summary>
/// <remarks>
/// Constraints that are disabled outright are reported by <see cref="DisabledObjectsRule"/> instead,
/// so that a single constraint does not produce two messages.
/// </remarks>
/// <seealso cref="Rule" />
/// <seealso cref="ITableRule" />
public class UnvalidatedConstraintsRule : Rule, ITableRule
{
    /// <summary>
    /// The reporting level this rule uses unless a caller overrides it: warning, because an
    /// unvalidated constraint describes data the database has never checked.
    /// </summary>
    public const RuleLevel DefaultLevel = RuleLevel.Warning;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnvalidatedConstraintsRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level, or <see langword="null" /> to use <see cref="DefaultLevel"/>.</param>
    public UnvalidatedConstraintsRule(RuleLevel? level = null)
        : base(RuleId, RuleTitle, level ?? DefaultLevel)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when enabled but unvalidated constraints are discovered.
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
    /// Analyses a database table. Reports messages when enabled but unvalidated constraints are discovered.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var result = new List<IRuleMessage>();

        var unvalidatedForeignKeys = table.ParentKeys
            .Select(static fk => fk.ChildKey)
            .Where(IsUnvalidated);
        foreach (var foreignKey in unvalidatedForeignKeys)
        {
            var ruleMessage = BuildUnvalidatedForeignKeyMessage(table.Name, foreignKey.Name);
            result.Add(ruleMessage);
        }

        table.PrimaryKey
            .Where(IsUnvalidated)
            .Map(pk => BuildUnvalidatedPrimaryKeyMessage(table.Name, pk.Name))
            .IfSome(result.Add);

        var unvalidatedUniqueKeys = table.UniqueKeys.Where(IsUnvalidated);
        foreach (var uniqueKey in unvalidatedUniqueKeys)
        {
            var ruleMessage = BuildUnvalidatedUniqueKeyMessage(table.Name, uniqueKey.Name);
            result.Add(ruleMessage);
        }

        var unvalidatedChecks = table.Checks.Where(IsUnvalidated);
        foreach (var check in unvalidatedChecks)
        {
            var ruleMessage = BuildUnvalidatedCheckConstraintMessage(table.Name, check.Name);
            result.Add(ruleMessage);
        }

        return result;
    }

    private static bool IsUnvalidated(IDatabaseConstraint constraint) => constraint.IsEnabled && !constraint.IsValidated;

    /// <summary>
    /// Builds a message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="foreignKeyName">The name of the foreign key, if available.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildUnvalidatedForeignKeyMessage(Identifier tableName, Option<Identifier> foreignKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageKeyName = GetConstraintNameSuffix(foreignKeyName);
        var messageText = $"The table '{tableName}' contains an unvalidated foreign key{messageKeyName}. The database will not rely upon it when planning queries. Consider validating the existing rows.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// Builds a message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="primaryKeyName">The name of the primary key, if available.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildUnvalidatedPrimaryKeyMessage(Identifier tableName, Option<Identifier> primaryKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageKeyName = GetConstraintNameSuffix(primaryKeyName);
        var messageText = $"The table '{tableName}' contains an unvalidated primary key{messageKeyName}. The database will not rely upon it when planning queries. Consider validating the existing rows.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// Builds a message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="uniqueKeyName">The name of the unique key, if available.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildUnvalidatedUniqueKeyMessage(Identifier tableName, Option<Identifier> uniqueKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageKeyName = GetConstraintNameSuffix(uniqueKeyName);
        var messageText = $"The table '{tableName}' contains an unvalidated unique key{messageKeyName}. The database will not rely upon it when planning queries. Consider validating the existing rows.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// Builds a message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="checkName">The name of the check constraint, if available.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildUnvalidatedCheckConstraintMessage(Identifier tableName, Option<Identifier> checkName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageCheckName = GetConstraintNameSuffix(checkName);
        var messageText = $"The table '{tableName}' contains an unvalidated check constraint{messageCheckName}. Existing rows are not known to satisfy it. Consider validating the existing rows.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// Formats a constraint name for embedding in a message, or an empty string when unnamed.
    /// </summary>
    /// <param name="constraintName">The name of the constraint, if available.</param>
    /// <returns>A quoted, space-prefixed constraint name, or an empty string.</returns>
    protected static string GetConstraintNameSuffix(Option<Identifier> constraintName)
    {
        return constraintName.Match(
            static name => " '" + name.LocalName + "'",
            static () => string.Empty
        );
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId => "SCHEMATIC0040";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle => "Unvalidated constraint present on a table.";
}
