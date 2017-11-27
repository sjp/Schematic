using System;
using System.Data;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Provides an identity to be used to determine whether a command is unique for a collection.
    /// </summary>
    public class CachingCommandIdentity
    {
        public CachingCommandIdentity(IDbCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var hashCode = 17;

            unchecked
            {
                hashCode = (hashCode * 23) + command.CommandText.GetHashCode();
                hashCode = (hashCode * 23) + command.CommandType.GetHashCode();

                var parameters = new DataParameterCollectionIdentity(command.Parameters);
                hashCode = (hashCode * 23) + parameters.Identity;
            }

            Identity = hashCode;
        }

        public override int GetHashCode() => Identity;

        public int Identity { get; }
    }
}