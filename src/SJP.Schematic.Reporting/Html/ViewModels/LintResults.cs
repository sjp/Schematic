using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    internal class LintResults : ITemplateParameter
    {
        public ReportTemplate Template { get; } = ReportTemplate.Lint;

        public IEnumerable<LintRule> LintRules
        {
            get => _lintRules;
            set => _lintRules = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<LintRule> _lintRules = Enumerable.Empty<LintRule>();

        public uint LintRulesCount => _lintRules.UCount();

        internal class LintRule
        {
            public LintRule(string ruleTitle, IEnumerable<string> messages)
            {
                RuleTitle = ruleTitle ?? throw new ArgumentNullException(nameof(ruleTitle));
                Messages = messages ?? throw new ArgumentNullException(nameof(messages));
            }

            public string RuleTitle { get; }

            public IEnumerable<string> Messages { get; }

            public uint MessageCount => Messages.UCount();
        }
    }
}
