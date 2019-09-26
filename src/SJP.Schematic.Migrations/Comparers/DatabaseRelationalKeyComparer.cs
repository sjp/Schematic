using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Comparers
{
    public class DatabaseRelationalKeyComparer : IEqualityComparer<IDatabaseRelationalKey>
    {
        public DatabaseRelationalKeyComparer(IEqualityComparer<IDatabaseKey> keyComparer)
        {
            KeyComparer = keyComparer ?? throw new ArgumentNullException(nameof(keyComparer));
        }

        protected IEqualityComparer<IDatabaseKey> KeyComparer { get; }

        public bool Equals(IDatabaseRelationalKey x, IDatabaseRelationalKey y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;

            return x.ChildTable == y.ChildTable
                && x.ParentTable == y.ParentTable
                && x.DeleteAction == y.DeleteAction
                && x.UpdateAction == y.UpdateAction
                && KeyComparer.Equals(x.ChildKey, y.ChildKey)
                && KeyComparer.Equals(x.ParentKey, y.ParentKey);
        }

        public int GetHashCode(IDatabaseRelationalKey obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCode.Combine(
                obj.ChildTable,
                obj.ParentTable,
                obj.DeleteAction,
                obj.UpdateAction,
                KeyComparer.GetHashCode(obj.ChildKey),
                KeyComparer.GetHashCode(obj.ParentKey)
            );
        }
    }
}
