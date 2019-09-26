using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class RenameRoutineComparer : EqualityComparer<RenameRoutineOperation>
    {
        public override bool Equals(RenameRoutineOperation x, RenameRoutineOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Routine.Name == y.Routine.Name
                && x.TargetName == y.TargetName;
        }

        public override int GetHashCode(RenameRoutineOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCode.Combine(
                obj.Routine.Name,
                obj.TargetName
            );
        }
    }
}
