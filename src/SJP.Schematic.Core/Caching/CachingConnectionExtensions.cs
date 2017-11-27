using System;
using System.Data;
using System.Data.Common;

namespace SJP.Schematic.Core.Caching
{
    public static class CachingExtensions
    {
        public static IDbConnection AsCachedConnection(this IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var dbConnection = connection as DbConnection ?? new DbConnectionAdapter(connection);
            return new CachingConnection(dbConnection);
        }

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