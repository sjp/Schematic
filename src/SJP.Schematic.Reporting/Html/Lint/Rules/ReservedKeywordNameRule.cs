﻿using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ReservedKeywordNameRule : Schematic.Lint.Rules.ReservedKeywordNameRule
{
    public ReservedKeywordNameRule(IDatabaseDialect dialect, RuleLevel level)
        : base(dialect, level)
    {
    }

    protected override IRuleMessage BuildTableMessage(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{tableUrl}\">{HttpUtility.HtmlEncode(tableName.ToVisibleName())}</a>";
        var messageText = $"The table {tableLink} is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildTableColumnMessage(Identifier tableName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{tableUrl}\">{HttpUtility.HtmlEncode(tableName.ToVisibleName())}</a>";
        var messageText = $"The table {tableLink} contains a column <code>{HttpUtility.HtmlEncode(columnName)}</code> which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildViewMessage(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var viewUrl = UrlRouter.GetViewUrl(viewName);
        var viewLink = $"<a href=\"{viewUrl}\">{HttpUtility.HtmlEncode(viewName.ToVisibleName())}</a>";
        var messageText = $"The view {viewLink} is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildViewColumnMessage(Identifier viewName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(viewName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var viewUrl = UrlRouter.GetViewUrl(viewName);
        var viewLink = $"<a href=\"{viewUrl}\">{HttpUtility.HtmlEncode(viewName.ToVisibleName())}</a>";
        var messageText = $"The view {viewLink} contains a column <code>{HttpUtility.HtmlEncode(columnName)}</code> which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildSequenceMessage(Identifier sequenceName)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        var sequenceUrl = UrlRouter.GetSequenceUrl(sequenceName);
        var sequenceLink = $"<a href=\"{sequenceUrl}\">{HttpUtility.HtmlEncode(sequenceName.ToVisibleName())}</a>";
        var messageText = $"The sequence {sequenceLink} is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildSynonymMessage(Identifier synonymName)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        var synonymUrl = UrlRouter.GetSynonymUrl(synonymName);
        var synonymLink = $"<a href=\"{synonymUrl}\">{HttpUtility.HtmlEncode(synonymName.ToVisibleName())}</a>";
        var messageText = $"The synonym {synonymLink} is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildRoutineMessage(Identifier routineName)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var routineUrl = UrlRouter.GetRoutineUrl(routineName);
        var routineLink = $"<a href=\"{routineUrl}\">{HttpUtility.HtmlEncode(routineName.ToVisibleName())}</a>";
        var messageText = $"The routine {routineLink} is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}