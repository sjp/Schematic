using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SJP.Schematic.Core;

/// <summary>
/// Not intended to be used directly. Used for internal purposes to get access to a database connection.
/// </summary>
public static class ConnectionRegistry
{
    private static readonly ConcurrentDictionary<Guid, WeakReference<IDbConnectionFactory>> ConnectionFactoryLookup = new();
    private static readonly ConditionalWeakTable<IDbConnectionFactory, string> ConnectionIdLookup = [];

    /// <summary>
    /// Registers a database connection factory by its identifier.
    /// </summary>
    /// <param name="connectionId">The connection identifier.</param>
    /// <param name="connectionFactory">A database connection factory.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <see langword="null" />.</exception>
    public static void RegisterConnection(Guid connectionId, IDbConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);

        ConnectionFactoryLookup.AddOrUpdate(
            connectionId,
            (_, connFactory) => new WeakReference<IDbConnectionFactory>(connFactory),
            (_, reference, connFactory) =>
            {
                reference.SetTarget(connFactory);
                return reference;
            },
            connectionFactory);
        ConnectionIdLookup.AddOrUpdate(connectionFactory, connectionId.ToString());
    }

    /// <summary>
    /// Tries to get a connection identifier. This will be available if the connection factory has been registered.
    /// </summary>
    /// <param name="connectionFactory">A database connection factory.</param>
    /// <param name="connectionId">The connection identifier.</param>
    /// <returns><see langword="true" /> if the connection has been registered and a connection identifier has been found; otherwise <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <see langword="null" />.</exception>
    public static bool TryGetConnectionId(IDbConnectionFactory connectionFactory, out Guid connectionId)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);

        if (ConnectionIdLookup.TryGetValue(connectionFactory, out var guidStr)
            && Guid.TryParse(guidStr, out var lookupId))
        {
            connectionId = lookupId;
            return true;
        }

        connectionId = Guid.Empty;
        return false;
    }
}