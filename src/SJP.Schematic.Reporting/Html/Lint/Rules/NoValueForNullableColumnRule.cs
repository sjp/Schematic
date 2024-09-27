﻿using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class NoValueForNullableColumnRule : Schematic.Lint.Rules.NoValueForNullableColumnRule
{
    public NoValueForNullableColumnRule(ISchematicConnection connection, RuleLevel level)
        : base(connection, level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{tableUrl}\">{HttpUtility.HtmlEncode(tableName.ToVisibleName())}</a>";
        var messageText = $"The table {tableLink} has a nullable column <code>{HttpUtility.HtmlEncode(columnName)}</code> whose values are always <code>NULL</code>. Consider removing the column.";

        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}