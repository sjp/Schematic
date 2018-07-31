using System;
using System.Data;
using System.Linq;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Provides an identity to be used for determining whether a parameter collection is unique for a command.
    /// </summary>
    public sealed class DataParameterCollectionIdentity : IEquatable<DataParameterCollectionIdentity>
    {
        /// <summary>
        /// Creates a <see cref="DataParameterCollectionIdentity"/> instance to create an identity for an <see cref="IDataParameterCollection"/>.
        /// </summary>
        /// <param name="collection">An <see cref="IDataParameterCollection"/> object.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c>.</exception>
        public DataParameterCollectionIdentity(IDataParameterCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var hashCode = 17;

            unchecked
            {
                var parameters = collection.OfType<IDbDataParameter>();
                foreach (var parameter in parameters)
                {
                    var hashParameter = new DataParameterIdentity(parameter);
                    hashCode = (hashCode * 23) + hashParameter.Identity;
                }
            }

            Identity = hashCode;
        }

        public override int GetHashCode() => Identity;

        public bool Equals(DataParameterCollectionIdentity other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other is null)
                return false;

            return Identity == other.Identity;
        }

        public override bool Equals(object obj) => Equals(obj as DataParameterCollectionIdentity);

        /// <summary>
        /// An integer value that represents a unique hash for a <see cref="IDataParameterCollection"/>.
        /// </summary>
        public int Identity { get; }
    }
}