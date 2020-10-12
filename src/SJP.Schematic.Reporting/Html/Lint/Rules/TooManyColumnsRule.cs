using System;
using System.Globalization;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal sealed class TooManyColumnsRule : Schematic.Lint.Rules.TooManyColumnsRule
    {
        public TooManyColumnsRule(RuleLevel level, uint columnLimit = 100)
            : base(level, columnLimit)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier tableName, int columnCount)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableUrl = UrlRouter.GetTableUrl(tableName);
            var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table { tableLink } has too many columns. It has { columnCount.ToString(CultureInfo.InvariantCulture) } columns.";

            return new RuleMessage(RuleId, RuleTitle, Level, messageText);
        }
    }
}
