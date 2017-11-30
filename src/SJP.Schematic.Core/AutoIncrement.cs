using System;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// A description of an autoincrementing sequence.
    /// </summary>
    public struct AutoIncrement : IAutoIncrement
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
    }
}
