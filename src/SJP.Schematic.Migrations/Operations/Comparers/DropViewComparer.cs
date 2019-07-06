using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class DropViewComparer : EqualityComparer<DropViewOperation>
    {
        public override bool Equals(DropViewOperation x, DropViewOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null ^ y is null)
                return false;

            return x.View.Name == y.View.Name;
        }

        public override int GetHashCode(DropViewOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return obj.View.Name.GetHashCode();
        }
    }
}
