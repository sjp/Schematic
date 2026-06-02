using System;
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

        var messageText = columnCount == 0
            ? $"The table {tableName.ToVisibleName()} has too few columns. It has no columns, consider adding more."
            : $"The table {tableName.ToVisibleName()} has too few columns. It has one column, consider adding more.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
