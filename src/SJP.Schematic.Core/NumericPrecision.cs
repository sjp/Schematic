using System;
using System.ComponentModel;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    public readonly struct NumericPrecision : INumericPrecision, IEquatable<INumericPrecision>
    {
        public NumericPrecision(int precision, int scale)
        {
            if (precision < 0)
                throw new ArgumentException("The precision must be non-negative.", nameof(precision));
            if (scale < 0)
                throw new ArgumentException("The precision must be non-negative.", nameof(scale));

            Precision = precision;
            Scale = scale;
        }

        public int Precision { get; }

        public int Scale { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj is NumericPrecision np)
                return Equals(np);

            return false;
        }

        public bool Equals(INumericPrecision other)
        {
            if (other == null)
                return false;

            return Precision == other.Precision
                && Scale == other.Scale;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => HashCodeBuilder.Combine(Precision, Scale);

        public static bool operator ==(in NumericPrecision left, in NumericPrecision right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in NumericPrecision left, in NumericPrecision right)
        {
            return !(left == right);
        }

        public static bool operator ==(INumericPrecision left, in NumericPrecision right)
        {
            if (left == null)
                return false;

            return right.Equals(left);
        }

        public static bool operator !=(INumericPrecision left, in NumericPrecision right)
        {
            if (left == null)
                return true;

            return !(left == right);
        }

        public static bool operator ==(in NumericPrecision left, INumericPrecision right)
        {
            if (right == null)
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(in NumericPrecision left, INumericPrecision right)
        {
            if (right == null)
                return true;

            return !(left == right);
        }
    }
}
