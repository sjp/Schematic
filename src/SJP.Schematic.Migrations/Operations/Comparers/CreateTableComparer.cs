using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class CreateTableComparer : EqualityComparer<CreateTableOperation>
    {
        public override bool Equals(CreateTableOperation x, CreateTableOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Table.Name == y.Table.Name;
        }

        public override int GetHashCode(CreateTableOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return obj.Table.Name.GetHashCode();
        }
    }
}
