using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class NullableBooleanColumnRule : Schematic.Lint.Rules.NullableBooleanColumnRule
{
    public NullableBooleanColumnRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, Identifier columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(columnName);

        var messageText = $"The table {tableName.ToVisibleName()} has a nullable boolean column '{columnName.LocalName}'. This introduces an ambiguous three-valued state (true, false, unknown); consider making it non-nullable with a default value.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
