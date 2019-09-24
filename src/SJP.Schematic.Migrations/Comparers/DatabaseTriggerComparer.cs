using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Comparers
{
    public class DatabaseTriggerComparer : IEqualityComparer<IDatabaseTrigger>
    {
        public bool Equals(IDatabaseTrigger x, IDatabaseTrigger y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;

            return x.Name == y.Name
                && x.Definition == y.Definition
                && x.IsEnabled == y.IsEnabled
                && x.QueryTiming == y.QueryTiming
                && x.TriggerEvent == y.TriggerEvent;
        }

        public int GetHashCode(IDatabaseTrigger obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(
                obj.Name,
                obj.Definition,
                obj.IsEnabled,
                obj.QueryTiming,
                obj.TriggerEvent
            );
        }
    }
}
