using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class RenameSynonymComparer : EqualityComparer<RenameSynonymOperation>
    {
        public override bool Equals(RenameSynonymOperation x, RenameSynonymOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null ^ y is null)
                return false;

            return x.Synonym.Name == y.Synonym.Name
                && x.TargetName == y.TargetName;
        }

        public override int GetHashCode(RenameSynonymOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(
                obj.Synonym.Name,
                obj.TargetName
            );
        }
    }
}
