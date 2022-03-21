using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class OrphanedTableRule : Schematic.Lint.Rules.OrphanedTableRule
{
    public OrphanedTableRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
        var messageText = $"The table { tableLink } is not related to any other table. Consider adding relations or removing the table.";

        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}