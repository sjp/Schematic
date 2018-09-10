using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal sealed class DisabledObjectsRule : Schematic.Lint.Rules.DisabledObjectsRule
    {
        public DisabledObjectsRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildDisabledForeignKeyMessage(Identifier tableName, string foreignKeyName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageKeyName = !foreignKeyName.IsNullOrWhiteSpace()
                ? " <code>" + HttpUtility.HtmlEncode(foreignKeyName) + "</code>"
                : string.Empty;

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table '{ tableLink }' contains a disabled foreign key{ messageKeyName }. Consider enabling or removing the foreign key.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildDisabledPrimaryKeyMessage(Identifier tableName, string primaryKeyName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageKeyName = !primaryKeyName.IsNullOrWhiteSpace()
                ? " <code>" + HttpUtility.HtmlEncode(primaryKeyName) + "</code>"
                : string.Empty;

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table '{ tableLink }' contains a disabled primary key{ messageKeyName }. Consider enabling or removing the primary key.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildDisabledUniqueKeyMessage(Identifier tableName, string uniqueKeyName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageKeyName = !uniqueKeyName.IsNullOrWhiteSpace()
                ? " <code>" + HttpUtility.HtmlEncode(uniqueKeyName) + "</code>"
                : string.Empty;

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table '{ tableLink }' contains a disabled unique key{ messageKeyName }. Consider enabling or removing the unique key.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildDisabledCheckConstraintMessage(Identifier tableName, string checkName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageCheckName = !checkName.IsNullOrWhiteSpace()
                ? " <code>" + HttpUtility.HtmlEncode(checkName) + "</code>"
                : string.Empty;

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table '{ tableLink }' contains a disabled check constraint{ messageCheckName }. Consider enabling or removing the check constraint.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildDisabledIndexMessage(Identifier tableName, string indexName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageIndexName = !indexName.IsNullOrWhiteSpace()
                ? " <code>" + HttpUtility.HtmlEncode(indexName) + "</code>"
                : string.Empty;

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table '{ tableLink }' contains a disabled index{ messageIndexName }. Consider enabling or removing the index.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected override IRuleMessage BuildDisabledTriggerMessage(Identifier tableName, string triggerName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageTriggerName = !triggerName.IsNullOrWhiteSpace()
                ? " <code>" + HttpUtility.HtmlEncode(triggerName) + "</code>"
                : string.Empty;

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table '{ tableLink }' contains a disabled trigger{ messageTriggerName }. Consider enabling or removing the trigger.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
