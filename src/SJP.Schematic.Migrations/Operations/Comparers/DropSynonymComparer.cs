using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class DropSynonymComparer : EqualityComparer<DropSynonymOperation>
    {
        public override bool Equals(DropSynonymOperation x, DropSynonymOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Synonym.Name == y.Synonym.Name;
        }

        public override int GetHashCode(DropSynonymOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return obj.Synonym.Name.GetHashCode();
        }
    }
}
