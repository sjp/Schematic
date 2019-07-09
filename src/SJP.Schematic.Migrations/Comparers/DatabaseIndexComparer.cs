using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Comparers
{
    public class DatabaseIndexComparer : IEqualityComparer<IDatabaseIndex>
    {
        public DatabaseIndexComparer(IEqualityComparer<IDatabaseColumn> columnComparer)
        {
            ColumnComparer = columnComparer ?? throw new ArgumentNullException(nameof(columnComparer));
        }

        protected IEqualityComparer<IDatabaseColumn> ColumnComparer { get; }

        public bool Equals(IDatabaseIndex x, IDatabaseIndex y)
        {
            if (x is null && y is null)
                return true;
            if (x is null ^ y is null)
                return false;

            var xColumnOrders = x.Columns.Select(c => c.Order).ToList();
            var yColumnOrders = y.Columns.Select(c => c.Order).ToList();

            var xColumnExpressions = x.Columns.Select(c => c.Expression).ToList();
            var yColumnExpressions = y.Columns.Select(c => c.Expression).ToList();

            var columnsEqual = xColumnOrders == yColumnOrders
                && xColumnExpressions == yColumnExpressions;

            var includedColumnsEqual = x.IncludedColumns.SequenceEqual(y.IncludedColumns);

            return x.Name == y.Name
                && x.IsEnabled == y.IsEnabled
                && x.IsUnique == y.IsUnique
                && columnsEqual
                && includedColumnsEqual;
        }

        public int GetHashCode(IDatabaseIndex obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var builder = new HashCodeBuilder();
            builder.Add(obj.Name);
            builder.Add(obj.IsEnabled);
            builder.Add(obj.IsUnique);

            foreach (var column in obj.Columns)
            {
                builder.Add(column.Order);
                builder.Add(column.Expression);
            }

            foreach (var includedColumn in obj.IncludedColumns)
                builder.Add(ColumnComparer.GetHashCode(includedColumn));

            return builder.ToHashCode();
        }
    }
}
