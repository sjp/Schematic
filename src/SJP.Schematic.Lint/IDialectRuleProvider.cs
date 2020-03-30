using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint
{
    /// <summary>
    /// A rule provider that provides rules scoped to a specific <see cref="IDatabaseDialect"/> instance.
    /// </summary>
    /// <typeparam name="TDialect">A database dialect type.</typeparam>
    /// <seealso cref="IRuleProvider"/>
    public interface IDialectRuleProvider<TDialect> where TDialect : notnull, IDatabaseDialect, new()
    {
        /// <summary>
        /// Retrieves the set of rules used to analyze a dialect's database objects for reporting.
        /// </summary>
        /// <param name="connection">A schematic connection.</param>
        /// <param name="level">The level used for reporting.</param>
        /// <returns>Rules used for analyzing a dialect's database objects.</returns>
        IEnumerable<IRule> GetRules(ISchematicConnection connection, RuleLevel level);
    }
}
