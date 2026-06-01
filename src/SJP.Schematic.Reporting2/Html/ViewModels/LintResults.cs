using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The lint summary payload (<c>data/lint.json</c>): lint messages grouped by the rule that
/// produced them. Each rule carries its title and the plain-text messages it raised.
/// </summary>
public sealed class LintResults : ITemplateParameter
{
    public LintResults(IEnumerable<LintRule> lintRules)
    {
        LintRules = lintRules ?? throw new ArgumentNullException(nameof(lintRules));
        LintRulesCount = lintRules.UCount();
    }

    [JsonIgnore]
    public ReportTemplate Template { get; } = ReportTemplate.Lint;

    public IEnumerable<LintRule> LintRules { get; }

    public uint LintRulesCount { get; }

    /// <summary>
    /// A group of lint messages produced by a single rule.
    /// </summary>
    public sealed class LintRule
    {
        public LintRule(string ruleTitle, IEnumerable<string> messages)
        {
            RuleTitle = ruleTitle ?? throw new ArgumentNullException(nameof(ruleTitle));
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
            MessageCount = messages.UCount();
        }

        public string RuleTitle { get; }

        public IEnumerable<string> Messages { get; }

        public uint MessageCount { get; }
    }
}
