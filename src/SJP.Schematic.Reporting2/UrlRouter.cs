using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting;

/// <summary>
/// Builds the in-app hash routes used by the React SPA. Every link is an absolute hash
/// fragment (e.g. <c>#/tables/&lt;safeKey&gt;</c>) so deep links survive being opened from disk
/// (<c>file://</c>), where there is no server to resolve clean URLs. <see cref="Identifier.ToSafeKey"/>
/// gives the 1:1 mapping used for filenames, bundle keys, and route params.
/// </summary>
internal static class UrlRouter
{
    public static string GetTableUrl(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return "#/tables/" + tableName.ToSafeKey();
    }

    public static string GetViewUrl(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return "#/views/" + viewName.ToSafeKey();
    }

    public static string GetSequenceUrl(Identifier sequenceName)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        return "#/sequences/" + sequenceName.ToSafeKey();
    }

    public static string GetSynonymUrl(Identifier synonymName)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        return "#/synonyms/" + synonymName.ToSafeKey();
    }

    public static string GetRoutineUrl(Identifier routineName)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return "#/routines/" + routineName.ToSafeKey();
    }

    public static string GetTriggerUrl(Identifier tableName, Identifier triggerName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(triggerName);

        // Triggers fold into the owning table's detail page (no per-trigger route).
        return "#/tables/" + tableName.ToSafeKey();
    }
}
