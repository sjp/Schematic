using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Comparers
{
    public class DatabaseSequenceComparer : IEqualityComparer<IDatabaseSequence>
    {
        public bool Equals(IDatabaseSequence x, IDatabaseSequence y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;

            return x.Name == y.Name
                && x.Cache == y.Cache
                && x.Cycle == y.Cycle
                && x.Increment == y.Increment
                && x.MaxValue == y.MaxValue
                && x.MinValue == y.MinValue
                && x.Start == y.Start;
        }

        public int GetHashCode(IDatabaseSequence obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCode.Combine(
                obj.Name,
                obj.Cache,
                obj.Cycle,
                obj.Increment,
                obj.MaxValue,
                obj.MinValue,
                obj.Start
            );
        }
    }
}
