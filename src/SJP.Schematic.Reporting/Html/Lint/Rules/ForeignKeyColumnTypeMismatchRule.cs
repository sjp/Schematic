using System;
using System.Web;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal sealed class ForeignKeyColumnTypeMismatchRule : Schematic.Lint.Rules.ForeignKeyColumnTypeMismatchRule
    {
        public ForeignKeyColumnTypeMismatchRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier childTableName, Identifier parentTableName)
        {
            if (childTableName == null)
                throw new ArgumentNullException(nameof(childTableName));
            if (parentTableName == null)
                throw new ArgumentNullException(nameof(parentTableName));

            var builder = StringBuilderCache.Acquire();
            builder.Append("A foreign key");

            foreignKeyName.IfSome(fkName =>
            {
                builder.Append(" <code>")
                    .Append(HttpUtility.HtmlEncode(fkName))
                    .Append("</code>");
            });

            var childTableLink = $"<a href=\"tables/{ childTableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(childTableName.ToVisibleName()) }</a>";
            var parentTableLink = $"<a href=\"tables/{ parentTableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(parentTableName.ToVisibleName()) }</a>";

            builder.Append(" from ")
                .Append(childTableLink)
                .Append(" to ")
                .Append(parentTableLink)
                .Append(" contains mismatching column types. These should be the same in order to ensure that foreign keys can always hold the same information as the target key.");

            var messageText = builder.GetStringAndRelease();
            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
