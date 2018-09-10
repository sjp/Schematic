using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal sealed class NoNonNullableColumnsPresentRule : Schematic.Lint.Rules.NoNonNullableColumnsPresentRule
    {
        public NoNonNullableColumnsPresentRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table { tableLink } has no not-nullable columns present. Consider adding one to ensure that each record contains data.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
