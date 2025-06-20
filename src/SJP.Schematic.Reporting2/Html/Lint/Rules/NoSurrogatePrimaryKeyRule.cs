using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class NoSurrogatePrimaryKeyRule : Schematic.Lint.Rules.NoSurrogatePrimaryKeyRule
{
    public NoSurrogatePrimaryKeyRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{tableUrl}\">{HttpUtility.HtmlEncode(tableName.ToVisibleName())}</a>";
        var messageText = $"The table {tableLink} has a multi-column primary key. Consider introducing a surrogate primary key.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}