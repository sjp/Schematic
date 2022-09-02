using System;
using System.Collections.Generic;
using System.Reflection;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint;

/// <summary>
/// Constructs a rule provider that returns the set of rules used for reporting. Differs from the default rules in that plugin-based rules can be used.
/// </summary>
/// <seealso cref="IRuleProvider" />
public sealed class ReportingRuleProvider : IRuleProvider
{
    /// <summary>
    /// Retrieves the set of rules used to analyze database objects for reporting.
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

        var ruleProvider = new RuleProviderBuilder()
            .AddRuleProvider<DefaultHtmlRuleProvider>()
            .AddRuleProvider<PluginProvider>()
            .Build();

        return ruleProvider.GetRules(connection, level);
    }

    // ensures rules in the lint assembly and the reporting assembly are ignored
    private sealed class PluginProvider : PluginRuleProvider
    {
        protected override bool TypeFilter(Type type, Type dialectType)
            => !string.Equals(type.Assembly.Location, ReportingAssembly.Location, StringComparison.OrdinalIgnoreCase);

        private static readonly Assembly ReportingAssembly = Assembly.GetExecutingAssembly();
    }
}