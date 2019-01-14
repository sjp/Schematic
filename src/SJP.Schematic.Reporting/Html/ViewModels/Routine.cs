using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    public sealed class Routine : ITemplateParameter
    {
        public Routine(
            Identifier routine,
            string rootPath,
            string definition
        )
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            Name = routine.ToVisibleName();
            RoutineUrl = routine.ToSafeKey();
            RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public ReportTemplate Template { get; } = ReportTemplate.Routine;

        public string RootPath { get; }

        public string Name { get; }

        public string RoutineUrl { get; }

        public string Definition { get; }
    }
}
