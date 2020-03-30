using System;
using System.Collections.Generic;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint
{
    /// <summary>
    /// Constructs a rule provider that returns no rules. Not intended to be used directly.
    /// </summary>
    /// <seealso cref="IRuleProvider" />
    public sealed class EmptyRuleProvider : IRuleProvider
    {
        /// <summary>
        /// Retrieves an empty set of rules used for reporting.
        /// </summary>
        /// <param name="connection">A schematic connection.</param>
        /// <param name="level">The level used for reporting.</param>
        /// <returns>An empty set of rules.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="level"/> does not have a valid enum value.</exception>
        public IEnumerable<IRule> GetRules(ISchematicConnection connection, RuleLevel level)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (!level.IsValid())
                throw new ArgumentException($"The { nameof(RuleLevel) } provided must be a valid enum.", nameof(level));

            return Array.Empty<IRule>();
        }
    }
}
