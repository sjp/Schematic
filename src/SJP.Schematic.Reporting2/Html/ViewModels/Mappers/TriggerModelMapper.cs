using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class TriggerModelMapper
{
    public Triggers.TriggerRow Map(Identifier tableName, IDatabaseTrigger trigger)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(trigger);

        return new Triggers.TriggerRow(
            tableName,
            trigger.Name,
            trigger.Definition,
            trigger.QueryTiming,
            trigger.TriggerEvent
        );
    }
}
