using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class DropColumnComparer : EqualityComparer<DropColumnOperation>
    {
        public override bool Equals(DropColumnOperation x, DropColumnOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null ^ y is null)
                return false;

            return x.Table.Name == y.Table.Name
                && x.Column.Name == y.Column.Name;
        }

        public override int GetHashCode(DropColumnOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(obj.Table.Name, obj.Column.Name);
        }
    }
}
