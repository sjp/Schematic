using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class DropTableComparer : EqualityComparer<DropTableOperation>
    {
        public override bool Equals(DropTableOperation x, DropTableOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Table.Name == y.Table.Name;
        }

        public override int GetHashCode(DropTableOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return obj.Table.Name.GetHashCode();
        }
    }
}
