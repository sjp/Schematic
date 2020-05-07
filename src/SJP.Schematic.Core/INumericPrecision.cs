namespace SJP.Schematic.Core
{
    /// <summary>
    /// Defines the precision of a numeric data type.
    /// </summary>
    public interface INumericPrecision
    {
        /// <summary>
        /// The number of digits in a number.
        /// </summary>
        /// <value>A numeric value for the precision.</value>
        int Precision { get; }

        /// <summary>
        /// The number of digits to the right of the decimal point in a number.
        /// </summary>
        /// <value>A numeric value for the scale.</value>
        int Scale { get; }
    }
}