using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class AlterTableComparer : EqualityComparer<AlterTableOperation>
    {
        public override bool Equals(AlterTableOperation x, AlterTableOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null ^ y is null)
                return false;

            return x.ExistingTable.Name == y.ExistingTable.Name;
        }

        public override int GetHashCode(AlterTableOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return obj.ExistingTable.Name.GetHashCode();
        }
    }
}
