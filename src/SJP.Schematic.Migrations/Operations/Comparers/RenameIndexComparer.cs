using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class RenameIndexComparer : EqualityComparer<RenameIndexOperation>
    {
        public override bool Equals(RenameIndexOperation x, RenameIndexOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Table.Name == y.Table.Name
                && x.Index.Name == y.Index.Name
                && x.TargetName == y.TargetName;
        }

        public override int GetHashCode(RenameIndexOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(
                obj.Table.Name,
                obj.Index.Name,
                obj.TargetName
            );
        }
    }
}
