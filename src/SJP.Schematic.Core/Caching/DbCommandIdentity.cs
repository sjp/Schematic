using System;
using System.Data;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Provides an identity to be used to determine whether a command is unique for a collection.
    /// </summary>
    public sealed class DbCommandIdentity : IEquatable<DbCommandIdentity>
    {
        /// <summary>
        /// Creates a <see cref="DbCommandIdentity"/> instance to create an identity for an <see cref="IDbCommand"/>.
        /// </summary>
        /// <param name="command">An <see cref="IDbCommand"/> object.</param>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is <c>null</c>.</exception>
        public DbCommandIdentity(IDbCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var hashCode = 17;

            unchecked
            {
                hashCode = (hashCode * 23) + (command.CommandText?.GetHashCode() ?? 0);
                hashCode = (hashCode * 23) + command.CommandType.GetHashCode();

                var parameters = new DataParameterCollectionIdentity(command.Parameters);
                hashCode = (hashCode * 23) + parameters.Identity;
            }

            Identity = hashCode;
        }

        public override int GetHashCode() => Identity;

        public bool Equals(DbCommandIdentity other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (ReferenceEquals(other, null))
                return false;

            return Identity == other.Identity;
        }

        public override bool Equals(object obj) => Equals(obj as DbCommandIdentity);

        /// <summary>
        /// An integer value that represents a unique hash for a <see cref="IDbCommand"/>.
        /// </summary>
        public int Identity { get; }
    }
}