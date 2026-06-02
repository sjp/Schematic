using System;
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

        var messageText = $"The table {tableName.ToVisibleName()} has a column '{columnName}' whose default value is NULL. Consider removing the default value on the column.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
