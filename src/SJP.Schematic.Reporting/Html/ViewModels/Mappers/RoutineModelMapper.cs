using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class RoutineModelMapper
    {
        public Routine Map(IDatabaseRoutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            return new Routine(routine.Name, "../", routine.Definition);
        }
    }
}
