using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class AlterColumnComparer : EqualityComparer<AlterColumnOperation>
    {
        public override bool Equals(AlterColumnOperation x, AlterColumnOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Table.Name == y.Table.Name
                && x.ExistingColumn.Name == y.ExistingColumn.Name;
        }

        public override int GetHashCode(AlterColumnOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCode.Combine(obj.Table.Name, obj.ExistingColumn.Name);
        }
    }
}
