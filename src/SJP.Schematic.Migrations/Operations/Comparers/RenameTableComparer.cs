using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class RenameTableComparer : EqualityComparer<RenameTableOperation>
    {
        public override bool Equals(RenameTableOperation x, RenameTableOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Table.Name == y.Table.Name
                && x.TargetName == y.TargetName;
        }

        public override int GetHashCode(RenameTableOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCode.Combine(
                obj.Table.Name,
                obj.TargetName
            );
        }
    }
}
