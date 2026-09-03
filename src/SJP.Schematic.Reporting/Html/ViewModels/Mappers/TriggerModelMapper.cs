using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class TriggerModelMapper
{
    public Triggers.TriggerRow Map(Identifier tableName, IDatabaseTrigger trigger)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(trigger);

        return Map(tableName, UrlRouter.GetTableUrl(tableName), trigger);
    }

    public Triggers.TriggerRow MapView(Identifier viewName, IDatabaseTrigger trigger)
    {
        ArgumentNullException.ThrowIfNull(viewName);
        ArgumentNullException.ThrowIfNull(trigger);

        return Map(viewName, UrlRouter.GetViewUrl(viewName), trigger);
    }

    private static Triggers.TriggerRow Map(Identifier objectName, string objectUrl, IDatabaseTrigger trigger)
    {
        return new Triggers.TriggerRow(
            objectName,
            objectUrl,
            trigger.Name,
            trigger.Definition,
            trigger.QueryTiming,
            trigger.TriggerEvent,
            trigger.Granularity,
            trigger.Condition,
            trigger.UpdateColumns
        );
    }
}
