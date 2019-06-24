using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class DropRoutineOperation : MigrationOperation
    {
        public DropRoutineOperation(IDatabaseRoutine routine)
        {
            Routine = routine ?? throw new ArgumentNullException(nameof(routine));
        }

        public IDatabaseRoutine Routine { get; }
    }
}
