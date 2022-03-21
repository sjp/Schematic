using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when a set of foreign key relationships forms a cycle.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class ForeignKeyRelationshipCycleRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKeyRelationshipCycleRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public ForeignKeyRelationshipCycleRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when a set of foreign key relationships forms a cycle.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <c>null</c>.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        if (tables == null)
            throw new ArgumentNullException(nameof(tables));

        var cycleDetector = new CycleDetector();
        var cycles = cycleDetector.GetCyclePaths(tables.ToList());

        return cycles.Select(BuildMessage).ToAsyncEnumerable();
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="cyclePath">A collection of table names that form a cycle.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="cyclePath"/> is <c>null</c>.</exception>
    protected virtual IRuleMessage BuildMessage(IReadOnlyCollection<Identifier> cyclePath)
    {
        if (cyclePath == null)
            throw new ArgumentNullException(nameof(cyclePath));

        var tableNames = cyclePath
            .Select(name => Identifier.CreateQualifiedIdentifier(name.Schema, name.LocalName).ToString())
            .Join(" -> ");
        var message = "Cycle found for the following path: " + tableNames;

        return new RuleMessage(RuleId, RuleTitle, Level, message);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0009";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Foreign key relationships contain a cycle.";
}