using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class AddColumnComparer : EqualityComparer<AddColumnOperation>
    {
        public override bool Equals(AddColumnOperation x, AddColumnOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null ^ y is null)
                return false;

            return x.Table.Name == y.Table.Name
                && x.Column.Name == y.Column.Name;
        }

        public override int GetHashCode(AddColumnOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(obj.Table.Name, obj.Column.Name);
        }
    }
}
