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
}