using System;
using System.Web;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class DisabledObjectsRule : Schematic.Lint.Rules.DisabledObjectsRule
{
    public DisabledObjectsRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildDisabledForeignKeyMessage(Identifier tableName, Option<Identifier> foreignKeyName)
    {
        var messageKeyName = foreignKeyName.Match(
            static name => " <code>" + HttpUtility.HtmlEncode(name.LocalName) + "</code>",
            static () => string.Empty
        );

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
        var messageText = $"The table { tableLink } contains a disabled foreign key{ messageKeyName }. Consider enabling or removing the foreign key.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildDisabledPrimaryKeyMessage(Identifier tableName, Option<Identifier> primaryKeyName)
    {
        var messageKeyName = primaryKeyName.Match(
               static name => " <code>" + HttpUtility.HtmlEncode(name.LocalName) + "</code>",
               static () => string.Empty
           );

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
        var messageText = $"The table { tableLink } contains a disabled primary key{ messageKeyName }. Consider enabling or removing the primary key.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildDisabledUniqueKeyMessage(Identifier tableName, Option<Identifier> uniqueKeyName)
    {
        var messageKeyName = uniqueKeyName.Match(
            static name => " <code>" + HttpUtility.HtmlEncode(name.LocalName) + "</code>",
            static () => string.Empty
        );

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
        var messageText = $"The table { tableLink } contains a disabled unique key{ messageKeyName }. Consider enabling or removing the unique key.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildDisabledCheckConstraintMessage(Identifier tableName, Option<Identifier> checkName)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));

        var messageCheckName = checkName.Match(
            static name => " <code>" + HttpUtility.HtmlEncode(name.LocalName) + "</code>",
            static () => string.Empty
        );

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
        var messageText = $"The table { tableLink } contains a disabled check constraint{ messageCheckName }. Consider enabling or removing the check constraint.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildDisabledIndexMessage(Identifier tableName, string? indexName)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));

        var messageIndexName = !indexName.IsNullOrWhiteSpace()
            ? " <code>" + HttpUtility.HtmlEncode(indexName) + "</code>"
            : string.Empty;

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
        var messageText = $"The table { tableLink } contains a disabled index{ messageIndexName }. Consider enabling or removing the index.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildDisabledTriggerMessage(Identifier tableName, string? triggerName)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));

        var messageTriggerName = !triggerName.IsNullOrWhiteSpace()
            ? " <code>" + HttpUtility.HtmlEncode(triggerName) + "</code>"
            : string.Empty;

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
        var messageText = $"The table { tableLink } contains a disabled trigger{ messageTriggerName }. Consider enabling or removing the trigger.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}