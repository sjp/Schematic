using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class AlterSequenceComparer : EqualityComparer<AlterSequenceOperation>
    {
        public override bool Equals(AlterSequenceOperation x, AlterSequenceOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.ExistingSequence.Name == y.ExistingSequence.Name;
        }

        public override int GetHashCode(AlterSequenceOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return obj.ExistingSequence.Name.GetHashCode();
        }
    }
}
