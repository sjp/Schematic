using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal sealed class ForeignKeyMissingRule : Schematic.Lint.Rules.ForeignKeyMissingRule
    {
        public ForeignKeyMissingRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(string columnName, Identifier tableName, Identifier targetTableName)
        {
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (targetTableName == null)
                throw new ArgumentNullException(nameof(targetTableName));

            var builder = StringBuilderCache.Acquire();

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var targetTableLink = $"<a href=\"tables/{ targetTableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(targetTableName.ToVisibleName()) }</a>";

            builder.Append("The table ")
                .Append(tableLink)
                .Append(" has a column <code>")
                .Append(columnName)
                .Append("</code> implying a relationship to ")
                .Append(targetTableLink)
                .Append(" which is missing a foreign key constraint.");

            var messageText = builder.GetStringAndRelease();
            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
