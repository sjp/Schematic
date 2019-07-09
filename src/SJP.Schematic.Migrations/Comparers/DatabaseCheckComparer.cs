using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Comparers
{
    public class DatabaseCheckComparer : IEqualityComparer<IDatabaseCheckConstraint>
    {
        public bool Equals(IDatabaseCheckConstraint x, IDatabaseCheckConstraint y)
        {
            if (x is null && y is null)
                return true;
            if (x is null ^ y is null)
                return false;

            return x.Name == y.Name
                && x.Definition == y.Definition
                && x.IsEnabled == y.IsEnabled;
        }

        public int GetHashCode(IDatabaseCheckConstraint obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(
                obj.Name,
                obj.Definition,
                obj.IsEnabled
            );
        }
    }
}
