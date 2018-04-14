using System;
using System.Data;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Provides an identity to be used for determining whether a parameter is unique within a command's parameter collection.
    /// </summary>
    public sealed class DataParameterIdentity : IEquatable<DataParameterIdentity>
    {
        /// <summary>
        /// Creates a <see cref="DataParameterIdentity"/> instance to create an identity for an <see cref="IDbDataParameter"/>.
        /// </summary>
        /// <param name="parameter">An <see cref="IDbDataParameter"/> object.</param>
        /// <exception cref="ArgumentNullException"><paramref name="parameter"/> is <c>null</c>.</exception>
        public DataParameterIdentity(IDbDataParameter parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            var hashCode = 17;

            unchecked
            {
                hashCode = (hashCode * 23) + (parameter.ParameterName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 23) + parameter.Direction.GetHashCode();
                hashCode = (hashCode * 23) + (parameter.Value?.GetHashCode() ?? 0);
            }

            Identity = hashCode;
        }

        /// <summary>
        /// Retrieves a hash code that is equal to the identity of the object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() => Identity;

        public bool Equals(DataParameterIdentity other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (ReferenceEquals(other, null))
                return false;

            return Identity == other.Identity;
        }

        public override bool Equals(object obj) => Equals(obj as DataParameterIdentity);

        /// <summary>
        /// An integer value that represents a unique hash for a <see cref="IDbDataParameter"/>.
        /// </summary>
        public int Identity { get; }
    }
}