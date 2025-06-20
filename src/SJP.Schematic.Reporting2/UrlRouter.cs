using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting;

internal static class UrlRouter
{
    public static string GetTableUrl(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return "tables/" + tableName.ToSafeKey() + ".html";
    }

    public static string GetViewUrl(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return "views/" + viewName.ToSafeKey() + ".html";
    }

    public static string GetSequenceUrl(Identifier sequenceName)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        return "sequences/" + sequenceName.ToSafeKey() + ".html";
    }

    public static string GetSynonymUrl(Identifier synonymName)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        return "synonyms/" + synonymName.ToSafeKey() + ".html";
    }

    public static string GetRoutineUrl(Identifier routineName)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return "routines/" + routineName.ToSafeKey() + ".html";
    }

    public static string GetTriggerUrl(Identifier tableName, Identifier triggerName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(triggerName);

        return "tables/"
            + tableName.ToSafeKey()
            + "/triggers/"
            + triggerName.ToSafeKey()
            + ".html";
    }
}