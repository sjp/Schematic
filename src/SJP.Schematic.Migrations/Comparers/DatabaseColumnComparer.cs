using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Comparers
{
    public class DatabaseColumnComparer : IEqualityComparer<IDatabaseColumn>
    {
        public DatabaseColumnComparer(IEqualityComparer<IDbType> typeComparer)
        {
            TypeComparer = typeComparer ?? throw new ArgumentNullException(nameof(typeComparer));
        }

        protected IEqualityComparer<IDbType> TypeComparer { get; }

        public bool Equals(IDatabaseColumn x, IDatabaseColumn y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;

            var autoIncrementEqual = x.AutoIncrement.Match(
                xai => y.AutoIncrement.Match(yai => xai.Increment == yai.Increment && xai.InitialValue == yai.InitialValue, () => false),
                () => y.AutoIncrement.IsNone
            );

            return x.Name == y.Name
                && x.IsComputed == y.IsComputed
                && x.IsNullable == y.IsNullable
                && TypeComparer.Equals(x.Type, y.Type)
                && autoIncrementEqual;
        }

        public int GetHashCode(IDatabaseColumn obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var builder = new HashCode();
            builder.Add(obj.Name);
            builder.Add(obj.IsComputed);
            builder.Add(obj.IsNullable);
            builder.Add(TypeComparer.GetHashCode(obj.Type));

            obj.AutoIncrement.Match(
                ai =>
                {
                    builder.Add(ai.Increment);
                    builder.Add(ai.InitialValue);
                },
                () => builder.Add(0)
            );

            return builder.ToHashCode();
        }
    }
}
