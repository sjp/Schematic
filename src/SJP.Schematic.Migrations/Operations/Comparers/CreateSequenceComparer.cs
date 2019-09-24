using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class CreateSequenceComparer : EqualityComparer<CreateSequenceOperation>
    {
        public override bool Equals(CreateSequenceOperation x, CreateSequenceOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Sequence.Name == y.Sequence.Name;
        }

        public override int GetHashCode(CreateSequenceOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return obj.Sequence.Name.GetHashCode();
        }
    }
}
