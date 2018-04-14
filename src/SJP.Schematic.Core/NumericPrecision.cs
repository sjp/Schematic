using System;

namespace SJP.Schematic.Core
{
    public struct NumericPrecision : IEquatable<NumericPrecision>
    {
        public NumericPrecision(int precision, int scale)
        {
            Precision = precision;
            Scale = scale;
        }

        public int Precision { get; }

        public int Scale { get; }

        public override bool Equals(object obj)
        {
            if (obj is NumericPrecision np)
                return Equals(np);

            return false;
        }

        public bool Equals(NumericPrecision other)
        {
            return Precision == other.Precision
                && Scale == other.Scale;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 23) + Precision.GetHashCode();
                return (hash * 23) + Scale.GetHashCode();
            }
        }

        public static bool operator ==(NumericPrecision left, NumericPrecision right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NumericPrecision left, NumericPrecision right)
        {
            return !(left == right);
        }
    }
}
