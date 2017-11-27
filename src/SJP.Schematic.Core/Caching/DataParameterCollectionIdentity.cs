using System;
using System.Data;
using System.Linq;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Provides an identity to be used for determining whether a parameter collection is unique for a command.
    /// </summary>
    public class DataParameterCollectionIdentity
    {
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

        public int Identity { get; }
    }
}