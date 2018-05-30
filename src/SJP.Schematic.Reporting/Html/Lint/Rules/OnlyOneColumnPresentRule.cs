using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal class OnlyOneColumnPresentRule : Schematic.Lint.Rules.OnlyOneColumnPresentRule
    {
        public OnlyOneColumnPresentRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier tableName, int columnCount)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";

            var messageText = columnCount == 0
                ? $"The table { tableLink } has too few columns. It has no columns, consider adding more."
                : $"The table { tableLink } has too few columns. It has one column, consider adding more.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
