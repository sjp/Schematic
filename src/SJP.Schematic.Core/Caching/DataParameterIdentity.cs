using System;
using System.Data;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Provides an identity to be used for determining whether a parameter is unique within a command's parameter collection.
    /// </summary>
    public class DataParameterIdentity
    {
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

        public override int GetHashCode() => Identity;

        public int Identity { get; }
    }
}