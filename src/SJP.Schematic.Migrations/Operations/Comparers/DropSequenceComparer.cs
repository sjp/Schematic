using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class DropSequenceComparer : EqualityComparer<DropSequenceOperation>
    {
        public override bool Equals(DropSequenceOperation x, DropSequenceOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null ^ y is null)
                return false;

            return x.Sequence.Name == y.Sequence.Name;
        }

        public override int GetHashCode(DropSequenceOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return obj.Sequence.Name.GetHashCode();
        }
    }
}
