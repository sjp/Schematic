using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class CreateSynonymComparer : EqualityComparer<CreateSynonymOperation>
    {
        public override bool Equals(CreateSynonymOperation x, CreateSynonymOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Synonym.Name == y.Synonym.Name;
        }

        public override int GetHashCode(CreateSynonymOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return obj.Synonym.Name.GetHashCode();
        }
    }
}
