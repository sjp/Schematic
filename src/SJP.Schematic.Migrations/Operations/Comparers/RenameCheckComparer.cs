using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class RenameCheckComparer : EqualityComparer<RenameCheckOperation>
    {
        public override bool Equals(RenameCheckOperation x, RenameCheckOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Table.Name == y.Table.Name
                && NameComparer.Equals(x.Check.Name, y.Check.Name)
                && x.Check.Definition == y.Check.Definition
                && x.TargetName == y.TargetName;
        }

        public override int GetHashCode(RenameCheckOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(
                obj.Table.Name,
                obj.Check.Name,
                obj.Check.Definition,
                obj.TargetName
            );
        }

        private static readonly IEqualityComparer<Option<Identifier>> NameComparer = new OptionalNameComparer();
    }
}
