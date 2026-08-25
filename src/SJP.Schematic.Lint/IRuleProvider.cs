using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint;

/// <summary>
/// Provides a set of linting rules for database object analysis.
/// </summary>
public interface IRuleProvider
{
    /// <summary>
    /// Retrieves the rules used to analyze database objects.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="level">The level used for reporting.</param>
    /// <returns>Rules used for analyzing database objects.</returns>
    IEnumerable<IRule> GetRules(ISchematicConnection connection, RuleLevel level);

    /// <summary>
    /// Retrieves the rules used to analyze database objects, each at its own default reporting
    /// level.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <returns>Rules used for analyzing database objects.</returns>
    /// <remarks>
    /// Prefer this over <see cref="GetRules(ISchematicConnection, RuleLevel)"/> when results are
    /// presented to a person: forcing every rule to one level flattens the severity signal that
    /// makes a long list of findings triageable.
    /// </remarks>
    IEnumerable<IRule> GetRules(ISchematicConnection connection);
}