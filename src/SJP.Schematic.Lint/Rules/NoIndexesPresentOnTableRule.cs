using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when tables have no indexes present.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class NoIndexesPresentOnTableRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoIndexesPresentOnTableRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public NoIndexesPresentOnTableRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when no indexes are present on tables.
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
    /// Analyses a database table. Reports messages when no indexes are present on tables.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var noIndexesPresent = table.PrimaryKey.IsNone
            && table.UniqueKeys.Count == 0
            && table.Indexes.Count == 0;

        return noIndexesPresent
            ? [BuildMessage(table.Name)]
            : Array.Empty<IRuleMessage>();
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageText = $"The table {tableName} does not have any indexes present, requiring table scans to access records. Consider introducing an index or a primary key or a unique key constraint.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0011";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "No indexes present on table.";
}