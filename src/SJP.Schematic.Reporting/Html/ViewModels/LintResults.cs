using System;
using System.Collections.Generic;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Internal. Not intended to be used outside of this assembly. Only required for templating.
/// </summary>
public sealed class LintResults : ITemplateParameter
{
    public LintResults(IEnumerable<LintRule> lintRules)
    {
        LintRules = lintRules ?? throw new ArgumentNullException(nameof(lintRules));
        LintRulesCount = lintRules.UCount();
    }

    public ReportTemplate Template { get; } = ReportTemplate.Lint;

    public IEnumerable<LintRule> LintRules { get; }

    public uint LintRulesCount { get; }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class LintRule
    {
        public LintRule(string ruleTitle, IEnumerable<HtmlString> messages)
        {
            RuleTitle = ruleTitle ?? throw new ArgumentNullException(nameof(ruleTitle));
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
            MessageCount = messages.UCount();
        }

        public string RuleTitle { get; }

        public IEnumerable<HtmlString> Messages { get; }

        public uint MessageCount { get; }
    }
}
