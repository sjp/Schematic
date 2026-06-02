using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class NoNonNullableColumnsPresentRule : Schematic.Lint.Rules.NoNonNullableColumnsPresentRule
{
    public NoNonNullableColumnsPresentRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageText = $"The table {tableName.ToVisibleName()} has no not-nullable columns present. Consider adding one to ensure that each record contains data.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
