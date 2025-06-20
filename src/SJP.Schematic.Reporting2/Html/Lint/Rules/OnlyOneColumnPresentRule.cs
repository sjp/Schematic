using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class OnlyOneColumnPresentRule : Schematic.Lint.Rules.OnlyOneColumnPresentRule
{
    public OnlyOneColumnPresentRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, int columnCount)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{tableUrl}\">{HttpUtility.HtmlEncode(tableName.ToVisibleName())}</a>";

        var messageText = columnCount == 0
            ? $"The table {tableLink} has too few columns. It has no columns, consider adding more."
            : $"The table {tableLink} has too few columns. It has one column, consider adding more.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}