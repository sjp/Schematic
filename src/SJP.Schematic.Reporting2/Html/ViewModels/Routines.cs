using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The routines summary payload (<c>data/routines.json</c>): the list of routines rendered by
/// the routines listing page.
/// </summary>
public sealed class Routines
{
    public Routines(IEnumerable<Main.Routine> routines)
    {
        if (routines.NullOrAnyNull())
            throw new ArgumentNullException(nameof(routines));

        RoutinesCount = routines.UCount();
        AllRoutines = routines;
    }

    public uint RoutinesCount { get; }

    public IEnumerable<Main.Routine> AllRoutines { get; }
}
