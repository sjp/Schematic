using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ColumnWithNullDefaultValueRule : Schematic.Lint.Rules.ColumnWithNullDefaultValueRule
{
    public ColumnWithNullDefaultValueRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{tableUrl}\">{HttpUtility.HtmlEncode(tableName.ToVisibleName())}</a>";
        var messageText = $"The table {tableLink} has a column <code>{HttpUtility.HtmlEncode(columnName)}</code> whose default value is <code>NULL</code>. Consider removing the default value on the column.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}