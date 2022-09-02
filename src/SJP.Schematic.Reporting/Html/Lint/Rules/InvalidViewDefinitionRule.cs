﻿using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class InvalidViewDefinitionRule : Schematic.Lint.Rules.InvalidViewDefinitionRule
{
    public InvalidViewDefinitionRule(ISchematicConnection connection, RuleLevel level)
        : base(connection, level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var viewUrl = UrlRouter.GetViewUrl(viewName);
        var viewLink = $"<a href=\"{ viewUrl }\">{ HttpUtility.HtmlEncode(viewName.ToVisibleName()) }</a>";
        var messageText = $"The view { viewLink } was unable to be queried. This may indicate an incorrect view definition.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}