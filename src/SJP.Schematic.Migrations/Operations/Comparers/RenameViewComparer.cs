using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class RenameViewComparer : EqualityComparer<RenameViewOperation>
    {
        public override bool Equals(RenameViewOperation x, RenameViewOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.View.Name == y.View.Name
                && x.TargetName == y.TargetName;
        }

        public override int GetHashCode(RenameViewOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCode.Combine(
                obj.View.Name,
                obj.TargetName
            );
        }
    }
}
