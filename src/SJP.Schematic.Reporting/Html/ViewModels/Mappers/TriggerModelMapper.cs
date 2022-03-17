using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class TriggerModelMapper
{
    public Trigger Map(Identifier tableName, IDatabaseTrigger trigger)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (trigger == null)
            throw new ArgumentNullException(nameof(trigger));

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
