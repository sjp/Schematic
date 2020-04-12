using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite
{
    public class SqliteDialect : DatabaseDialect
    {
        public static Task<IDbConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            if (connectionString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(connectionString));

            return CreateConnectionAsyncCore(connectionString, cancellationToken);
        }

        private static async Task<IDbConnection> CreateConnectionAsyncCore(string connectionString, CancellationToken cancellationToken)
        {
            var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }

        public override Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return GetIdentifierDefaultsAsyncCore();
        }

        private static Task<IIdentifierDefaults> GetIdentifierDefaultsAsyncCore()
        {
            var identifierDefaults = new IdentifierDefaults(null, null, DefaultSchema);
            return Task.FromResult<IIdentifierDefaults>(identifierDefaults);
        }

        private const string DefaultSchema = "main";

        public override Task<string> GetDatabaseDisplayVersionAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return GetDatabaseDisplayVersionAsyncCore(connection, cancellationToken);
        }

        private static async Task<string> GetDatabaseDisplayVersionAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
        {
            var versionStr = await connection.DbConnection.ExecuteScalarAsync<string>(DatabaseDisplayVersionQuerySql, cancellationToken).ConfigureAwait(false);
            return "SQLite " + versionStr;
        }

        public override Task<Version> GetDatabaseVersionAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return GetDatabaseVersionAsyncCore(connection, cancellationToken);
        }

        private static async Task<Version> GetDatabaseVersionAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
        {
            var versionStr = await connection.DbConnection.ExecuteScalarAsync<string>(DatabaseDisplayVersionQuerySql, cancellationToken).ConfigureAwait(false);
            return Version.Parse(versionStr);
        }

        private const string DatabaseDisplayVersionQuerySql = "select sqlite_version()";

        public override Task<IRelationalDatabase> GetRelationalDatabaseAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return GetRelationalDatabaseAsyncCore(connection);
        }

        private static async Task<IRelationalDatabase> GetRelationalDatabaseAsyncCore(ISchematicConnection connection)
        {
            var identifierDefaults = await GetIdentifierDefaultsAsyncCore().ConfigureAwait(false);
            return new SqliteRelationalDatabase(connection, identifierDefaults, new ConnectionPragma(connection));
        }

        public override Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
            => Task.FromResult<IRelationalDatabaseCommentProvider>(new EmptyRelationalDatabaseCommentProvider());

        public override IDependencyProvider GetDependencyProvider() => new SqliteDependencyProvider();

        public override bool IsReservedKeyword(string text)
        {
            if (text.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(text));

            return Keywords.Contains(text);
        }

        // https://www.sqlite.org/lang_keywords.html
        private static readonly IEnumerable<string> Keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ABORT",
            "ACTION",
            "ADD",
            "AFTER",
            "ALL",
            "ALTER",
            "ANALYZE",
            "AND",
            "AS",
            "ASC",
            "ATTACH",
            "AUTOINCREMENT",
            "BEFORE",
            "BEGIN",
            "BETWEEN",
            "BY",
            "CASCADE",
            "CASE",
            "CAST",
            "CHECK",
            "COLLATE",
            "COLUMN",
            "COMMIT",
            "CONFLICT",
            "CONSTRAINT",
            "CREATE",
            "CROSS",
            "CURRENT_DATE",
            "CURRENT_TIME",
            "CURRENT_TIMESTAMP",
            "DATABASE",
            "DEFAULT",
            "DEFERRABLE",
            "DEFERRED",
            "DELETE",
            "DESC",
            "DETACH",
            "DISTINCT",
            "DROP",
            "EACH",
            "ELSE",
            "END",
            "ESCAPE",
            "EXCEPT",
            "EXCLUSIVE",
            "EXISTS",
            "EXPLAIN",
            "FAIL",
            "FOR",
            "FOREIGN",
            "FROM",
            "FULL",
            "GLOB",
            "GROUP",
            "HAVING",
            "IF",
            "IGNORE",
            "IMMEDIATE",
            "IN",
            "INDEX",
            "INDEXED",
            "INITIALLY",
            "INNER",
            "INSERT",
            "INSTEAD",
            "INTERSECT",
            "INTO",
            "IS",
            "ISNULL",
            "JOIN",
            "KEY",
            "LEFT",
            "LIKE",
            "LIMIT",
            "MATCH",
            "NATURAL",
            "NO",
            "NOT",
            "NOTNULL",
            "NULL",
            "OF",
            "OFFSET",
            "ON",
            "OR",
            "ORDER",
            "OUTER",
            "PLAN",
            "PRAGMA",
            "PRIMARY",
            "QUERY",
            "RAISE",
            "RECURSIVE",
            "REFERENCES",
            "REGEXP",
            "REINDEX",
            "RELEASE",
            "RENAME",
            "REPLACE",
            "RESTRICT",
            "RIGHT",
            "ROLLBACK",
            "ROW",
            "SAVEPOINT",
            "SELECT",
            "SET",
            "TABLE",
            "TEMP",
            "TEMPORARY",
            "THEN",
            "TO",
            "TRANSACTION",
            "TRIGGER",
            "UNION",
            "UNIQUE",
            "UPDATE",
            "USING",
            "VACUUM",
            "VALUES",
            "VIEW",
            "VIRTUAL",
            "WHEN",
            "WHERE",
            "WITH",
            "WITHOUT"
        };

        public override string QuoteName(Identifier name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (name.Schema != null)
            {
                return QuoteIdentifier(name.Schema)
                    + "."
                    + QuoteIdentifier(name.LocalName);
            }
            else
            {
                return QuoteIdentifier(name.LocalName);
            }
        }

        public override IDbTypeProvider TypeProvider => InnerTypeProvider;

        private static readonly IDbTypeProvider InnerTypeProvider = new SqliteDbTypeProvider();
    }
}
