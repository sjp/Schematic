using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class EmptyRoutineDefinitionRule : Schematic.Lint.Rules.EmptyRoutineDefinitionRule
{
    public EmptyRoutineDefinitionRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier routineName)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var messageText = $"The routine {routineName.ToVisibleName()} has an empty definition. Consider removing it if it is unused, or restoring its body if the definition was lost.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
