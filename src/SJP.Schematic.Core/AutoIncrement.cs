using System;
using System.ComponentModel;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// A description of an auto-incrementing sequence.
    /// </summary>
    public readonly struct AutoIncrement : IAutoIncrement, IEquatable<AutoIncrement>, IEquatable<IAutoIncrement>
    {
        /// <summary>
        /// Creates a description of an auto-incrementing sequence.
        /// </summary>
        /// <param name="initialValue">The starting value of the sequence.</param>
        /// <param name="increment">The value incremented to the current value on each new row.</param>
        /// <exception cref="ArgumentException"><paramref name="increment"/> is zero.</exception>
        public AutoIncrement(decimal initialValue, decimal increment)
        {
            InitialValue = initialValue;

            if (increment == 0)
                throw new ArgumentException("The increment value must be non-zero.", nameof(increment));

            Increment = increment;
        }

        /// <summary>
        /// The starting value of the sequence.
        /// </summary>
        public decimal InitialValue { get; }

        /// <summary>
        /// The value incremented to the current value for each new row.
        /// </summary>
        public decimal Increment { get; }

        public bool Equals(AutoIncrement other)
        {
            return InitialValue == other.InitialValue
                && Increment == other.Increment;
        }

        public bool Equals(IAutoIncrement other)
        {
            if (other == null)
                return false;

            return InitialValue == other.InitialValue
                && Increment == other.Increment;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj is AutoIncrement ai)
                return Equals(ai);
            if (obj is IAutoIncrement iai)
                return Equals(iai);

            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => HashCode.Combine(InitialValue, Increment);

        public static bool operator ==(in AutoIncrement left, in AutoIncrement right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in AutoIncrement left, in AutoIncrement right)
        {
            return !(left == right);
        }

        public static bool operator ==(IAutoIncrement left, in AutoIncrement right)
        {
            if (left == null)
                return false;

            return right.Equals(left);
        }

        public static bool operator !=(IAutoIncrement left, in AutoIncrement right)
        {
            if (left == null)
                return true;

            return !(left == right);
        }

        public static bool operator ==(in AutoIncrement left, IAutoIncrement right)
        {
            if (right == null)
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(in AutoIncrement left, IAutoIncrement right)
        {
            if (right == null)
                return true;

            return !(left == right);
        }
    }
}
