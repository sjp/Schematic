using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Comparers
{
    public class DatabaseViewComparer : IEqualityComparer<IDatabaseView>
    {
        public bool Equals(IDatabaseView x, IDatabaseView y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;

            return x.Name == y.Name
                && x.IsMaterialized == y.IsMaterialized
                && x.Definition == y.Definition;
        }

        public int GetHashCode(IDatabaseView obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCode.Combine(
                obj.Name,
                obj.IsMaterialized,
                obj.Definition
            );
        }
    }
}
