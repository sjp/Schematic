using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class RenameTriggerComparer : EqualityComparer<RenameTriggerOperation>
    {
        public override bool Equals(RenameTriggerOperation x, RenameTriggerOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null ^ y is null)
                return false;

            return x.Table.Name == y.Table.Name
                && x.Trigger.Name == y.Trigger.Name
                && x.TargetName == y.TargetName;
        }

        public override int GetHashCode(RenameTriggerOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(
                obj.Table.Name,
                obj.Trigger.Name,
                obj.TargetName
            );
        }
    }
}
