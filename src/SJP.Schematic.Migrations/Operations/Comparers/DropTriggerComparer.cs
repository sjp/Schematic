using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class DropTriggerComparer : EqualityComparer<DropTriggerOperation>
    {
        public override bool Equals(DropTriggerOperation x, DropTriggerOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Table.Name == y.Table.Name
                && x.Trigger.Definition == y.Trigger.Definition
                && NameComparer.Equals(x.Trigger.Name, y.Trigger.Name);
        }

        public override int GetHashCode(DropTriggerOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(
                obj.Table.Name,
                obj.Trigger.Definition,
                NameComparer.GetHashCode(obj.Trigger.Name)
            );
        }

        private static readonly IEqualityComparer<Option<Identifier>> NameComparer = new OptionalNameComparer();
    }
}
