using System;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Convenience extension methods for working with <see cref="IDbConnectionFactory"/> objects.
/// </summary>
public static class DbConnectionFactoryExtensions
{
    /// <summary>
    /// Wraps a connection factory with an equivalent that caches connections that are created a given connection factory.
    /// </summary>
    /// <param name="connectionFactory">A connection factory to be cached.</param>
    /// <returns>A <see cref="CachingConnectionFactory"/> instance that caches connections created from the given connection factory.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>.</exception>
    public static IDbConnectionFactory AsCachingFactory(this IDbConnectionFactory connectionFactory)
    {
        if (connectionFactory == null)
            throw new ArgumentNullException(nameof(connectionFactory));

        return new CachingConnectionFactory(connectionFactory);
    }
}
