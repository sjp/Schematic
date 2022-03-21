using System;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting;

internal static class ConnectionExtensions
{
    public static Task<ulong> GetRowCountAsync(this IDbConnectionFactory connection, IDatabaseDialect dialect, Identifier tableName, CancellationToken cancellationToken)
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