using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Comparers
{
    public class DbTypeComparer : IEqualityComparer<IDbType>
    {
        public bool Equals(IDbType x, IDbType y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;

            var collationsEqual = x.Collation.Match(
                xc => y.Collation.Match(yc => xc.LocalName == yc.LocalName, () => false),
                () => y.Collation.IsNone
            );
            var numericPrecisionEqual = x.NumericPrecision.Match(
                xnp => y.NumericPrecision.Match(
                    ynp => xnp.Precision == ynp.Precision && xnp.Scale == ynp.Scale,
                    () => false
                ),
                () => y.NumericPrecision.IsNone
            );

            return collationsEqual
                && x.DataType == y.DataType
                && x.IsFixedLength == y.IsFixedLength
                && x.MaxLength == y.MaxLength
                && numericPrecisionEqual;
        }

        public int GetHashCode(IDbType obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return HashCodeBuilder.Combine(
                obj.Collation,
                obj.DataType,
                obj.IsFixedLength,
                obj.MaxLength,
                obj.NumericPrecision
            );
        }
    }
}
