using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class NoIndexesPresentOnTableRule : Schematic.Lint.Rules.NoIndexesPresentOnTableRule
{
    public NoIndexesPresentOnTableRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
        var messageText = $"The table { tableLink } does not have any indexes present, requiring table scans to access records. Consider introducing an index or a primary key or a unique key constraint.";

        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}