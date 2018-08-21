using System;
using System.Data;
using System.Linq;

namespace SJP.Schematic.Core.Caching
{
    public static class DbCommandIdentityProvider
    {
        public static int GetIdentity(IDbCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var hashCode = 17;

            unchecked
            {
                hashCode = (hashCode * 23) + (command.CommandText?.GetHashCode() ?? 0);
                hashCode = (hashCode * 23) + command.CommandType.GetHashCode();

                var parametersIdentity = GetIdentity(command.Parameters);
                return (hashCode * 23) + parametersIdentity;
            }
        }

        private static int GetIdentity(IDataParameterCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var hashCode = 17;

            unchecked
            {
                var parameters = collection.OfType<IDbDataParameter>();
                foreach (var parameter in parameters)
                {
                    var parameterIdentity = GetIdentity(parameter);
                    hashCode = (hashCode * 23) + parameterIdentity;
                }
            }

            return hashCode;
        }

        private static int GetIdentity(IDbDataParameter parameter)
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

            return hashCode;
        }
    }
}
