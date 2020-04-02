using System;
using System.Collections.Concurrent;
using System.Data;
using System.Runtime.CompilerServices;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Not intended to be used directly. Used for internal purposes to get access to a database connection.
    /// </summary>
    public static class ConnectionRegistry
    {
        private static readonly ConcurrentDictionary<Guid, WeakReference<IDbConnection>> ConnectionLookup = new ConcurrentDictionary<Guid, WeakReference<IDbConnection>>();
        private static readonly ConditionalWeakTable<IDbConnection, string> ConnectionIdLookup = new ConditionalWeakTable<IDbConnection, string>();

        /// <summary>
        /// Registers a database connection by its identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="connection">A database connection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public static void RegisterConnection(Guid connectionId, IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            ConnectionLookup.AddOrUpdate(
                connectionId,
                new WeakReference<IDbConnection>(connection),
                (_, reference) =>
                {
                    reference.SetTarget(connection);
                    return reference;
                });
            ConnectionIdLookup.AddOrUpdate(connection, connectionId.ToString());
        }

        /// <summary>
        /// Tries to get a connection identifier. This will be available if the connection has been registered.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="connectionId">The connection identifier.</param>
        /// <returns><c>true</c> if the connection has been registered and a connection identifier has been found; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public static bool TryGetConnectionId(IDbConnection connection, out Guid connectionId)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (ConnectionIdLookup.TryGetValue(connection, out var guidStr)
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
