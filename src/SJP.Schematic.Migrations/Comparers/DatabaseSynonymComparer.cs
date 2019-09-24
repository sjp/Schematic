using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Comparers
{
    public class DatabaseSynonymComparer : IEqualityComparer<IDatabaseSynonym>
    {
        public bool Equals(IDatabaseSynonym x, IDatabaseSynonym y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;

            return x.Name == y.Name
                && x.Target == y.Target;
        }

        public int GetHashCode(IDatabaseSynonym obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(
                obj.Name,
                obj.Target
            );
        }
    }
}
