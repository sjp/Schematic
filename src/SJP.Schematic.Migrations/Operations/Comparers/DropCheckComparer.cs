using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class DropCheckComparer : EqualityComparer<DropCheckOperation>
    {
        public override bool Equals(DropCheckOperation x, DropCheckOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null ^ y is null)
                return false;

            return x.Table.Name == y.Table.Name
                && x.Check.Definition == y.Check.Definition
                && NameComparer.Equals(x.Check.Name, y.Check.Name);
        }

        public override int GetHashCode(DropCheckOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(
                obj.Table.Name,
                obj.Check.Definition,
                NameComparer.GetHashCode(obj.Check.Name)
            );
        }

        private static readonly IEqualityComparer<Option<Identifier>> NameComparer = new OptionalNameComparer();
    }
}
