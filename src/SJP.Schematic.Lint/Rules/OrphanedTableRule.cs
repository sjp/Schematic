using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when a table is orphaned has is related to no other tables.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class OrphanedTableRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrphanedTableRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public OrphanedTableRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when a table is orphaned (i.e. it has no child or parent foreign key relationships).
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
    /// Analyses a database table. Reports messages when the table is orphaned (i.e. it has no child or parent foreign key relationships).
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
    protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        if (table == null)
            throw new ArgumentNullException(nameof(table));

        if (table.ParentKeys.Count > 0)
            return Array.Empty<IRuleMessage>();

        if (table.ChildKeys.Count > 0)
            return Array.Empty<IRuleMessage>();

        var message = BuildMessage(table.Name);
        return new[] { message };
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));

        var messageText = $"The table { tableName } is not related to any other table. Consider adding relations or removing the table.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0016";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "No relations on a table. The table is orphaned.";
}