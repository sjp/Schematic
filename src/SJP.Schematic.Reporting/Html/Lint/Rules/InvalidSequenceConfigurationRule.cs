using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class InvalidSequenceConfigurationRule : Schematic.Lint.Rules.InvalidSequenceConfigurationRule
{
    public InvalidSequenceConfigurationRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier sequenceName, string reason)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        var messageText = $"The sequence {sequenceName.ToVisibleName()} {reason}";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
