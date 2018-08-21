using System;
using System.Text;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal class ForeignKeyColumnTypeMismatchRule : Schematic.Lint.Rules.ForeignKeyColumnTypeMismatchRule
    {
        public ForeignKeyColumnTypeMismatchRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(string foreignKeyName, Identifier childTableName, Identifier parentTableName)
        {
            if (childTableName == null)
                throw new ArgumentNullException(nameof(childTableName));
            if (parentTableName == null)
                throw new ArgumentNullException(nameof(parentTableName));

            var builder = StringBuilderCache.Acquire();
            builder.Append("A foreign key");
            if (!foreignKeyName.IsNullOrWhiteSpace())
            {
                builder.Append(" <code>")
                    .Append(HttpUtility.HtmlEncode(foreignKeyName))
                    .Append("</code>");
            }

            var childTableLink = $"<a href=\"tables/{ childTableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(childTableName.ToVisibleName()) }</a>";
            var parentTableLink = $"<a href=\"tables/{ parentTableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(parentTableName.ToVisibleName()) }</a>";

            builder.Append(" from ")
                .Append(childTableLink)
                .Append(" to ")
                .Append(parentTableLink)
                .Append(" contains mismatching column types. These should be the same in order to ensure that foreign keys can always hold the same information as the target key.");

            var messageText = StringBuilderCache.GetStringAndRelease(builder);
            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
