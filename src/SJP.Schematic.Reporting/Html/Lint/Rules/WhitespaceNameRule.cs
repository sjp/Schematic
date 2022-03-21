using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class WhitespaceNameRule : Schematic.Lint.Rules.WhitespaceNameRule
{
    public WhitespaceNameRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildTableMessage(Identifier tableName)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
        var messageText = $"The table { tableLink } contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildTableColumnMessage(Identifier tableName, string columnName)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (columnName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(columnName));

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
        var messageText = $"The table { tableLink } contains a column <code>{ HttpUtility.HtmlEncode(columnName) }</code> which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildViewMessage(Identifier viewName)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        var viewUrl = UrlRouter.GetViewUrl(viewName);
        var viewLink = $"<a href=\"{ viewUrl }\">{ HttpUtility.HtmlEncode(viewName.ToVisibleName()) }</a>";
        var messageText = $"The view { viewLink } contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildViewColumnMessage(Identifier viewName, string columnName)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));
        if (columnName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(columnName));

        var viewUrl = UrlRouter.GetViewUrl(viewName);
        var viewLink = $"<a href=\"{ viewUrl }\">{ HttpUtility.HtmlEncode(viewName.ToVisibleName()) }</a>";
        var messageText = $"The view { viewLink } contains a column <code>{ HttpUtility.HtmlEncode(columnName) }</code> which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildSequenceMessage(Identifier sequenceName)
    {
        if (sequenceName == null)
            throw new ArgumentNullException(nameof(sequenceName));

        var sequenceUrl = UrlRouter.GetSequenceUrl(sequenceName);
        var sequenceLink = $"<a href=\"{ sequenceUrl }\">{ HttpUtility.HtmlEncode(sequenceName.ToVisibleName()) }</a>";
        var messageText = $"The sequence { sequenceLink } contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildSynonymMessage(Identifier synonymName)
    {
        if (synonymName == null)
            throw new ArgumentNullException(nameof(synonymName));

        var synonymUrl = UrlRouter.GetSynonymUrl(synonymName);
        var synonymLink = $"<a href=\"{ synonymUrl }\">{ HttpUtility.HtmlEncode(synonymName.ToVisibleName()) }</a>";
        var messageText = $"The synonym { synonymLink } contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildRoutineMessage(Identifier routineName)
    {
        if (routineName == null)
            throw new ArgumentNullException(nameof(routineName));

        var routineUrl = UrlRouter.GetRoutineUrl(routineName);
        var routineLink = $"<a href=\"{ routineUrl }\">{ HttpUtility.HtmlEncode(routineName.ToVisibleName()) }</a>";
        var messageText = $"The routine { routineLink } contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}