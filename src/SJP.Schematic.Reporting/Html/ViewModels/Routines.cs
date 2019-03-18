using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Routines : ITemplateParameter
    {
        public Routines(IEnumerable<Main.Routine> routines)
        {
            if (routines == null || routines.AnyNull())
                throw new ArgumentNullException(nameof(routines));

            RoutinesCount = routines.UCount();
            RoutinesTableClass = RoutinesCount > 0 ? CssClasses.DataTableClass : string.Empty;
            AllRoutines = routines;
        }

        public ReportTemplate Template { get; } = ReportTemplate.Routines;

        public uint RoutinesCount { get; }

        public HtmlString RoutinesTableClass { get; }

        public IEnumerable<Main.Routine> AllRoutines { get; }
    }
}
