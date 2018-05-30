using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal class WhitespaceNameRule : Schematic.Lint.Rules.WhitespaceNameRule
    {
        public WhitespaceNameRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildTableMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table { tableLink } contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildTableColumnMessage(Identifier tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table { tableLink } contains a column <code>{ HttpUtility.HtmlEncode(columnName) }</code> which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildViewMessage(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var viewLink = $"<a href=\"tables/{ viewName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(viewName.ToVisibleName()) }</a>";
            var messageText = $"The view { viewLink } contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildViewColumnMessage(Identifier viewName, string columnName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var viewLink = $"<a href=\"tables/{ viewName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(viewName.ToVisibleName()) }</a>";
            var messageText = $"The view { viewLink } contains a column <code>{ HttpUtility.HtmlEncode(columnName) }</code> which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildSequenceMessage(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var messageText = $"The sequence { HttpUtility.HtmlEncode(sequenceName.ToVisibleName()) } contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildSynonymMessage(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var messageText = $"The synonym { HttpUtility.HtmlEncode(synonymName.ToVisibleName()) } contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
