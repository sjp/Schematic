using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class TriggerWithNoEnabledEventsRule : Schematic.Lint.Rules.TriggerWithNoEnabledEventsRule
{
    public TriggerWithNoEnabledEventsRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, Identifier triggerName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(triggerName);

        var messageText = $"The table {tableName.ToVisibleName()} has a trigger '{triggerName.LocalName}' that is not bound to any event (INSERT, UPDATE or DELETE), so it can never fire. Consider removing it or binding it to an event.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
