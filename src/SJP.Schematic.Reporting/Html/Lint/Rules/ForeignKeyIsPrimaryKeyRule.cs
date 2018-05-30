using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal class ForeignKeyIsPrimaryKeyRule : Schematic.Lint.Rules.ForeignKeyIsPrimaryKeyRule
    {
        public ForeignKeyIsPrimaryKeyRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(string foreignKeyName, Identifier childTableName)
        {
            if (childTableName == null)
                throw new ArgumentNullException(nameof(childTableName));

            var builder = new StringBuilder("A foreign key");
            if (!foreignKeyName.IsNullOrWhiteSpace())
            {
                builder.Append(" <code>")
                    .Append(HttpUtility.HtmlEncode(foreignKeyName))
                    .Append("</code>");
            }

            var childTableLink = $"<a href=\"tables/{ childTableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(childTableName.ToVisibleName()) }</a>";
            builder.Append(" on ")
                .Append(childTableLink)
                .Append(" contains the same column set as the target key.");

            return new RuleMessage(RuleTitle, Level, builder.ToString());
        }
    }
}
