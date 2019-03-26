using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal sealed class PrimaryKeyColumnNotFirstColumnRule : Schematic.Lint.Rules.PrimaryKeyColumnNotFirstColumnRule
    {
        public PrimaryKeyColumnNotFirstColumnRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableUrl = UrlRouter.GetTableUrl(tableName);
            var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table { tableLink } has a primary key whose column is not the first column in the table.";

            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
