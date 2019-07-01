﻿using System;
using System.ComponentModel;

namespace SJP.Schematic.Core
{
    public struct NumericPrecision : INumericPrecision, IEquatable<INumericPrecision>
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

        public static bool operator ==(INumericPrecision left, NumericPrecision right)
        {
            if (left == null)
                return false;

            return right.Equals(left);
        }

        public static bool operator !=(INumericPrecision left, NumericPrecision right)
        {
            if (left == null)
                return true;

            return !(left == right);
        }

        public static bool operator ==(NumericPrecision left, INumericPrecision right)
        {
            if (right == null)
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(NumericPrecision left, INumericPrecision right)
        {
            if (right == null)
                return true;

            return !(left == right);
        }
    }
}
