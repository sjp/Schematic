using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class RenameSequenceComparer : EqualityComparer<RenameSequenceOperation>
    {
        public override bool Equals(RenameSequenceOperation x, RenameSequenceOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Sequence.Name == y.Sequence.Name
                && x.TargetName == y.TargetName;
        }

        public override int GetHashCode(RenameSequenceOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(
                obj.Sequence.Name,
                obj.TargetName
            );
        }
    }
}
