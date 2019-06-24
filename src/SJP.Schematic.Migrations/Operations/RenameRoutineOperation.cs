using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class RenameRoutineOperation : MigrationOperation
    {
        public RenameRoutineOperation(IDatabaseRoutine routine, Identifier targetName)
        {
            Routine = routine ?? throw new ArgumentNullException(nameof(routine));
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        }

        public IDatabaseRoutine Routine { get; }

        public Identifier TargetName { get; }
    }
}
