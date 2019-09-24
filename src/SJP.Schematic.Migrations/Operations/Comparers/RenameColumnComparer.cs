using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class RenameColumnComparer : EqualityComparer<RenameColumnOperation>
    {
        public override bool Equals(RenameColumnOperation x, RenameColumnOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Table.Name == y.Table.Name
                && x.Column.Name == y.Column.Name
                && x.TargetName == y.TargetName;
        }

        public override int GetHashCode(RenameColumnOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(
                obj.Table.Name,
                obj.Column.Name,
                obj.TargetName
            );
        }
    }
}
