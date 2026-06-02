using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class AutoIncrementColumnNotInKeyRule : Schematic.Lint.Rules.AutoIncrementColumnNotInKeyRule
{
    public AutoIncrementColumnNotInKeyRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, Identifier columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(columnName);

        var messageText = $"The table {tableName.ToVisibleName()} has an auto-incrementing column '{columnName.LocalName}' that is not part of any primary or unique key. This is often unintended.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
