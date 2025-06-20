using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class PrimaryKeyNotIntegerRule : Schematic.Lint.Rules.PrimaryKeyNotIntegerRule
{
    public PrimaryKeyNotIntegerRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{tableUrl}\">{HttpUtility.HtmlEncode(tableName.ToVisibleName())}</a>";
        var messageText = $"The table {tableLink} has a primary key which is not a single-column whose type is an integer.";

        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}