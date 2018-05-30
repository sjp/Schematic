using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal class TooManyColumnsRule : Schematic.Lint.Rules.TooManyColumnsRule
    {
        public TooManyColumnsRule(RuleLevel level, uint columnLimit = 100)
            : base(level, columnLimit)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier tableName, int columnCount)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table { tableLink } has too many columns. It has { columnCount.ToString() } columns.";

            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
