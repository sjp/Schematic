using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Not intended to be used directly. Used for internal purposes to get access to a database connection.
    /// </summary>
    public static class ConnectionRegistry
    {
        private static readonly ConcurrentDictionary<Guid, WeakReference<IDbConnectionFactory>> ConnectionFactoryLookup = new ConcurrentDictionary<Guid, WeakReference<IDbConnectionFactory>>();
        private static readonly ConditionalWeakTable<IDbConnectionFactory, string> ConnectionIdLookup = new ConditionalWeakTable<IDbConnectionFactory, string>();

        /// <summary>
        /// Registers a database connection factory by its identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="connectionFactory">A database connection factory.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>.</exception>
        public static void RegisterConnection(Guid connectionId, IDbConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));

            ConnectionFactoryLookup.AddOrUpdate(
                connectionId,
                new WeakReference<IDbConnectionFactory>(connectionFactory),
                (_, reference) =>
                {
                    reference.SetTarget(connectionFactory);
                    return reference;
                });
            ConnectionIdLookup.AddOrUpdate(connectionFactory, connectionId.ToString());
        }

        /// <summary>
        /// Tries to get a connection identifier. This will be available if the connection factory has been registered.
        /// </summary>
        /// <param name="connectionFactory">A database connection factory.</param>
        /// <param name="connectionId">The connection identifier.</param>
        /// <returns><c>true</c> if the connection has been registered and a connection identifier has been found; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>.</exception>
        public static bool TryGetConnectionId(IDbConnectionFactory connectionFactory, out Guid connectionId)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));

            if (ConnectionIdLookup.TryGetValue(connectionFactory, out var guidStr)
                && Guid.TryParse(guidStr, out var lookupId))
            {
                connectionId = lookupId;
                return true;
            }
            else
            {
                connectionId = Guid.Empty;
                return false;
            }
        }
    }
}
