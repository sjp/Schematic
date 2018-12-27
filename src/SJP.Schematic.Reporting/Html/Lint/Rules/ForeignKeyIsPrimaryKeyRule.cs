using System;
using System.Web;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal sealed class ForeignKeyIsPrimaryKeyRule : Schematic.Lint.Rules.ForeignKeyIsPrimaryKeyRule
    {
        public ForeignKeyIsPrimaryKeyRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier childTableName)
        {
            if (childTableName == null)
                throw new ArgumentNullException(nameof(childTableName));

            var builder = StringBuilderCache.Acquire();
            builder.Append("A foreign key");

            foreignKeyName.IfSome(fkName =>
            {
                builder.Append(" <code>")
                    .Append(HttpUtility.HtmlEncode(fkName))
                    .Append("</code>");
            });

            var childTableLink = $"<a href=\"tables/{ childTableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(childTableName.ToVisibleName()) }</a>";
            builder.Append(" on ")
                .Append(childTableLink)
                .Append(" contains the same column set as the target key.");

            var message = builder.GetStringAndRelease();
            return new RuleMessage(RuleTitle, Level, message);
        }
    }
}
