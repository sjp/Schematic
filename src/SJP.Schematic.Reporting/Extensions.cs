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

    internal static class UrlRouter
    {
        public static string GetTableUrl(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return "tables/" + tableName.ToSafeKey() + ".html";
        }

        public static string GetViewUrl(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return "views/" + viewName.ToSafeKey() + ".html";
        }

        public static string GetSequenceUrl(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return "sequences/" + sequenceName.ToSafeKey() + ".html";
        }

        public static string GetSynonymUrl(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return "synonyms/" + synonymName.ToSafeKey() + ".html";
        }

        public static string GetRoutineUrl(Identifier routineName)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return "routines/" + routineName.ToSafeKey() + ".html";
        }
    }
}
