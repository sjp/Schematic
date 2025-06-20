using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class TriggerModelMapper
{
    public Trigger Map(Identifier tableName, IDatabaseTrigger trigger)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(trigger);

        return new Trigger(
            tableName,
            trigger.Name,
            string.Empty,
            trigger.Definition,
            trigger.QueryTiming,
            trigger.TriggerEvent
        );
    }
}