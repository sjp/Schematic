using System;
using System.ComponentModel;
using System.Globalization;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Dot
{
    /// <summary>
    /// An object representing a typical RGB color
    /// </summary>
    public sealed class RgbColor : IEquatable<RgbColor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RgbColor"/> class.
        /// </summary>
        /// <param name="hex">The hexadecimal.</param>
        /// <exception cref="ArgumentNullException">hex</exception>
        public RgbColor(string hex)
        {
            if (hex.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(hex));

            // validate hex string
            var hexOnly = hex.StartsWith("#", StringComparison.Ordinal) ? hex.Substring(1) : hex;
            var r = Convert.ToByte(hexOnly.Substring(0, 2), 16);
            var g = Convert.ToByte(hexOnly.Substring(2, 2), 16);
            var b = Convert.ToByte(hexOnly.Substring(4, 2), 16);

            _hex = ToRgbHex(r, g, b);
            _hashCode = _hex.GetHashCode(StringComparison.Ordinal);
        }

        private static string ToRgbHex(byte red, byte green, byte blue)
        {
            var r = red.ToString("X2", CultureInfo.InvariantCulture);
            var g = green.ToString("X2", CultureInfo.InvariantCulture);
            var b = blue.ToString("X2", CultureInfo.InvariantCulture);

            return string.Concat("#", r, g, b);
        }

        /// <summary>
        /// Returns a hex string representing this color (including # prefix).
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents the color of instance.
        /// </returns>
        public override string ToString() => _hex;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => _hashCode;

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return obj is RgbColor color && Equals(color);
        }

        /// <summary>
        /// Indicates whether the current <see cref="RgbColor"/> is equal to another <see cref="RgbColor"/>.
        /// </summary>
        /// <param name="other">An <see cref="RgbColor"/> to compare with this color.</param>
        /// <returns><c>true</c> if the current color is equal to the <paramref name="other">other</paramref> parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(RgbColor other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(_hex, other.ToString(), StringComparison.Ordinal);
        }

        private readonly string _hex;
        private readonly int _hashCode;
    }
}
