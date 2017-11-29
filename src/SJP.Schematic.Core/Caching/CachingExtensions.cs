using System;
using System.Data;
using System.Data.Common;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Extension methods to make creation of caching objects easier.
    /// </summary>
    public static class CachingExtensions
    {
        /// <summary>
        /// Creates a caching connection whose results will be stored in an in-memory cache.
        /// </summary>
        /// <param name="connection">A connection whose results will be cached.</param>
        /// <returns>An <see cref="IDbConnection"/> instance that will cache query results.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public static IDbConnection AsCachedConnection(this IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var dbConnection = connection as DbConnection ?? new DbConnectionAdapter(connection);
            return new CachingConnection(dbConnection);
        }

        /// <summary>
        /// Creates a caching connection whose results will be stored in a provided caching store.
        /// </summary>
        /// <param name="connection">A connection whose results will be cached.</param>
        /// <param name="cacheStore">A caching store, where query results will be stored.</param>
        /// <returns>An <see cref="IDbConnection"/> instance that will cache query results.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="cacheStore"/> is <c>null</c>.</exception>
        public static IDbConnection AsCachedConnection(this IDbConnection connection, ICacheStore<int, DataTable> cacheStore)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (cacheStore == null)
                throw new ArgumentNullException(nameof(cacheStore));

            var dbConnection = connection as DbConnection ?? new DbConnectionAdapter(connection);
            return new CachingConnection(dbConnection, cacheStore);
        }
    }
}