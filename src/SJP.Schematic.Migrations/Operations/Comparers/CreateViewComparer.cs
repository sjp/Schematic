using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class CreateViewComparer : EqualityComparer<CreateViewOperation>
    {
        public override bool Equals(CreateViewOperation x, CreateViewOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.View.Name == y.View.Name;
        }

        public override int GetHashCode(CreateViewOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return obj.View.Name.GetHashCode();
        }
    }
}
