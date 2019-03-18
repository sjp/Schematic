using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal sealed class ReservedKeywordNameRule : Schematic.Lint.Rules.ReservedKeywordNameRule
    {
        public ReservedKeywordNameRule(IDatabaseDialect dialect, RuleLevel level)
            : base(dialect, level)
        {
        }

        protected override IRuleMessage BuildTableMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table { tableLink } is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildTableColumnMessage(Identifier tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table { tableLink } contains a column <code>{ HttpUtility.HtmlEncode(columnName) }</code> which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildViewMessage(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var viewLink = $"<a href=\"views/{ viewName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(viewName.ToVisibleName()) }</a>";
            var messageText = $"The view { viewLink } is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildViewColumnMessage(Identifier viewName, string columnName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var viewLink = $"<a href=\"views/{ viewName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(viewName.ToVisibleName()) }</a>";
            var messageText = $"The view { viewLink } contains a column <code>{  HttpUtility.HtmlEncode(columnName) }</code> which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildSequenceMessage(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var sequenceLink = $"<a href=\"sequences/{ sequenceName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(sequenceName.ToVisibleName()) }</a>";
            var messageText = $"The sequence { sequenceLink } is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildSynonymMessage(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var synonymLink = $"<a href=\"synonyms/{ synonymName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(synonymName.ToVisibleName()) }</a>";
            var messageText = $"The synonym { synonymLink } is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildRoutineMessage(Identifier routineName)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var routineLink = $"<a href=\"routines/{ routineName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(routineName.ToVisibleName()) }</a>";
            var messageText = $"The routine { routineLink } is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
