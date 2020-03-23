using System;
using System.Collections.Concurrent;
using System.Data;
using System.Runtime.CompilerServices;

namespace SJP.Schematic.Core
{
    public static class ConnectionRegistry
    {
        private static readonly ConcurrentDictionary<Guid, WeakReference<IDbConnection>> ConnectionLookup = new ConcurrentDictionary<Guid, WeakReference<IDbConnection>>();
        private static readonly ConditionalWeakTable<IDbConnection, string> ConnectionIdLookup = new ConditionalWeakTable<IDbConnection, string>();

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
