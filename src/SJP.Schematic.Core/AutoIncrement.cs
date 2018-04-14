using System;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// A description of an autoincrementing sequence.
    /// </summary>
    public struct AutoIncrement : IAutoIncrement, IEquatable<AutoIncrement>
    {
        /// <summary>
        /// Creates a description of an autoincrementing sequence.
        /// </summary>
        /// <param name="initialValue">The starting value of the sequnce.</param>
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

        public override bool Equals(object obj)
        {
            if (obj is AutoIncrement ai)
                return Equals(ai);

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 23) + InitialValue.GetHashCode();
                return (hash * 23) + Increment.GetHashCode();
            }
        }

        public static bool operator ==(AutoIncrement left, AutoIncrement right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AutoIncrement left, AutoIncrement right)
        {
            return !(left == right);
        }
    }
}
