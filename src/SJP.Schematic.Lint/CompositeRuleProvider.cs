using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint;

/// <summary>
/// Composes a rule provider from a set of <see cref="IRuleProvider"/> instances. Enables multiple rules to be combined together.
/// </summary>
/// <seealso cref="IRuleProvider" />
public class CompositeRuleProvider : IRuleProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeRuleProvider"/> class.
    /// </summary>
    /// <param name="ruleProviders">Rule providers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="ruleProviders"/> is <c>null</c>.</exception>
    public CompositeRuleProvider(IEnumerable<IRuleProvider> ruleProviders)
    {
        RuleProviders = ruleProviders ?? throw new ArgumentNullException(nameof(ruleProviders));
    }

    /// <summary>
    /// Rule providers.
    /// </summary>
    /// <value>The set of rule providers used to generate lint rules.</value>
    protected IEnumerable<IRuleProvider> RuleProviders { get; }

    /// <summary>
    /// Retrieves the rules used to analyze database objects.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="level">The level used for reporting.</param>
    /// <returns>Rules used for analyzing database objects.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="level"/> does not have a valid enum value.</exception>
    public IEnumerable<IRule> GetRules(ISchematicConnection connection, RuleLevel level)
    {
        ArgumentNullException.ThrowIfNull(connection);
        if (!level.IsValid())
            throw new ArgumentException($"The { nameof(RuleLevel) } provided must be a valid enum.", nameof(level));

        return RuleProviders
            .SelectMany(rp => rp.GetRules(connection, level))
            .ToList();
    }
}