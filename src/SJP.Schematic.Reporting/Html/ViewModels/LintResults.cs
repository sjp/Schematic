using System;
using System.Collections.Generic;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
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
}
