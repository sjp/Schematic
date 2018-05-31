using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal class PrimaryKeyNotIntegerRule : Schematic.Lint.Rules.PrimaryKeyNotIntegerRule
    {
        public PrimaryKeyNotIntegerRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table { tableLink } has a primary key which is not a single-column whose type is an integer.";

            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
