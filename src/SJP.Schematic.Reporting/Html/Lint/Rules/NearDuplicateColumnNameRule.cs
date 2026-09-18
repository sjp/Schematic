using System;
using System.Globalization;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class NearDuplicateColumnNameRule : Schematic.Lint.Rules.NearDuplicateColumnNameRule
{
    public NearDuplicateColumnNameRule(RuleLevel? level = null)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, string columnName, string suggestedColumnName, int otherTableCount)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);
        ArgumentException.ThrowIfNullOrWhiteSpace(suggestedColumnName);

        var messageText = $"The column '{columnName}' in the table {tableName.ToVisibleName()} is spelled almost identically to '{suggestedColumnName}', which is used by {otherTableCount.ToString(CultureInfo.InvariantCulture)} other tables. Consider whether this is a misspelling.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }
}
