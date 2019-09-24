using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class CreateRoutineComparer : EqualityComparer<CreateRoutineOperation>
    {
        public override bool Equals(CreateRoutineOperation x, CreateRoutineOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Routine.Name == y.Routine.Name;
        }

        public override int GetHashCode(CreateRoutineOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return obj.Routine.Name.GetHashCode();
        }
    }
}
