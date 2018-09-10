using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal sealed class CandidateKeyMissingRule : Schematic.Lint.Rules.CandidateKeyMissingRule
    {
        public CandidateKeyMissingRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table { tableLink } has no candidate (primary or unique) keys. Consider adding one to ensure records are unique.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
