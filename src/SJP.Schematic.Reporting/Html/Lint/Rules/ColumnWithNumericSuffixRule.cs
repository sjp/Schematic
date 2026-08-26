using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ColumnWithNumericSuffixRule : Schematic.Lint.Rules.ColumnWithNumericSuffixRule
{
    public ColumnWithNumericSuffixRule(RuleLevel? level = null)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var messageText = $"The table {tableName.ToVisibleName()} has a column '{columnName}' with a numeric suffix, indicating denormalization.";

        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }
}
