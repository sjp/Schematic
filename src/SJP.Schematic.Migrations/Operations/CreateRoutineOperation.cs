using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class CreateRoutineOperation : MigrationOperation
    {
        public CreateRoutineOperation(IDatabaseRoutine routine)
        {
            Routine = routine ?? throw new ArgumentNullException(nameof(routine));
        }

        public IDatabaseRoutine Routine { get; }
    }
}
