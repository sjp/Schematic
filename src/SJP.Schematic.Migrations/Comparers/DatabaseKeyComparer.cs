using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Comparers
{
    public class DatabaseKeyComparer : IEqualityComparer<IDatabaseKey>
    {
        public DatabaseKeyComparer(IEqualityComparer<IDatabaseColumn> columnComparer)
        {
            ColumnComparer = columnComparer ?? throw new ArgumentNullException(nameof(columnComparer));
        }

        protected IEqualityComparer<IDatabaseColumn> ColumnComparer { get; }

        public bool Equals(IDatabaseKey x, IDatabaseKey y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;

            var columnsEqual = x.Columns.SequenceEqual(y.Columns);

            return x.Name == y.Name
                && x.IsEnabled == y.IsEnabled
                && x.KeyType == y.KeyType
                && columnsEqual;
        }

        public int GetHashCode(IDatabaseKey obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var builder = new HashCodeBuilder();
            builder.Add(obj.Name);
            builder.Add(obj.IsEnabled);
            builder.Add(obj.KeyType);

            foreach (var column in obj.Columns)
                builder.Add(ColumnComparer.GetHashCode(column));

            return builder.ToHashCode();
        }
    }
}
