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

namespace SJP.Schematic.Sqlite
{
    public class SqliteDialect : DatabaseDialect
    {
        public SqliteDialect(IDbConnection connection)
            : base(connection)
        {
        }

        public static Task<IDbConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
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

        public override Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var identifierDefaults = new IdentifierDefaultsBuilder()
                .WithSchema(DefaultSchema)
                .Build();
            return Task.FromResult(identifierDefaults);
        }

        private const string DefaultSchema = "main";

        public override async Task<string> GetDatabaseDisplayVersionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var versionStr = await Connection.ExecuteScalarAsync<string>(DatabaseDisplayVersionQuerySql, cancellationToken).ConfigureAwait(false);
            return "SQLite " + versionStr;
        }

        public override async Task<Version> GetDatabaseVersionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var versionStr = await Connection.ExecuteScalarAsync<string>(DatabaseDisplayVersionQuerySql, cancellationToken).ConfigureAwait(false);
            return Version.Parse(versionStr);
        }

        private const string DatabaseDisplayVersionQuerySql = "select sqlite_version()";

        public override async Task<IRelationalDatabase> GetRelationalDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var identifierDefaults = await GetIdentifierDefaultsAsync(cancellationToken).ConfigureAwait(false);
            return new SqliteRelationalDatabase(this, Connection, identifierDefaults);
        }

        public override Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(CancellationToken cancellationToken = default(CancellationToken))
            => Task.FromResult<IRelationalDatabaseCommentProvider>(new EmptyRelationalDatabaseCommentProvider());

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
