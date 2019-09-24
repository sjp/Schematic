using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Comparers
{
    public class DatabaseRoutineComparer : IEqualityComparer<IDatabaseRoutine>
    {
        public bool Equals(IDatabaseRoutine x, IDatabaseRoutine y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;

            return x.Name == y.Name
                && x.Definition == y.Definition;
        }

        public int GetHashCode(IDatabaseRoutine obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(
                obj.Name,
                obj.Definition
            );
        }
    }
}
