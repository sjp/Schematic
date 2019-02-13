using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System.Data;
using System.Threading;

namespace SJP.Schematic.Reporting
{
    internal static class CollectionExtensions
    {
        public static uint UCount<T>(this IReadOnlyCollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var count = collection.Count;
            if (count < 0)
                throw new ArgumentException("The given collection has a negative count. This is not supported.", nameof(collection));

            return (uint)count;
        }

        public static uint UCount<T>(this IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var count = collection.Count();
            if (count < 0)
                throw new ArgumentException("The given collection has a negative count. This is not supported.", nameof(collection));

            return (uint)count;
        }
    }

    internal static class ConnectionExtensions
    {
        public static Task<ulong> GetRowCountAsync(this IDbConnection connection, IDatabaseDialect dialect, Identifier tableName, CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var name = Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName);
            var quotedName = dialect.QuoteName(name);

            var query = "SELECT COUNT(*) FROM " + quotedName;
            return connection.ExecuteScalarAsync<ulong>(query, cancellationToken);
        }
    }

    internal static class DatabaseKeyExtensions
    {
        public static int GetKeyHash(this IDatabaseKey key, Identifier tableName)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            unchecked
            {
                var hash = 17;
                hash = (hash * 23) + tableName.GetHashCode();
                hash = (hash * 23) + key.KeyType.GetHashCode();
                foreach (var column in key.Columns)
                    hash = (hash * 23) + (column.Name?.LocalName?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
