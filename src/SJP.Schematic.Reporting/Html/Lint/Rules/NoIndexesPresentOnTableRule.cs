using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal sealed class NoIndexesPresentOnTableRule : Schematic.Lint.Rules.NoIndexesPresentOnTableRule
    {
        public NoIndexesPresentOnTableRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table { tableLink } does not have any indexes present, requiring table scans to access records. Consider introducing an index or a primary key or a unique key constraint.";

            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
