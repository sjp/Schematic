using System;
using System.Globalization;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class TooManyColumnsRule : Schematic.Lint.Rules.TooManyColumnsRule
{
    public TooManyColumnsRule(RuleLevel level, uint columnLimit = 100)
        : base(level, columnLimit)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, int columnCount)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageText = $"The table {tableName.ToVisibleName()} has too many columns. It has {columnCount.ToString(CultureInfo.InvariantCulture)} columns.";

        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
