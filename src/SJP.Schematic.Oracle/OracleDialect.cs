using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Comments;
using SJP.Schematic.Oracle.Query;
using SJP.Schematic.Oracle.QueryResult;

namespace SJP.Schematic.Oracle
{
    /// <summary>
    /// A database dialect specific to SQL Server.
    /// </summary>
    /// <seealso cref="DatabaseDialect" />
    public class OracleDialect : DatabaseDialect
    {
        /// <summary>
        /// Quotes a string identifier, e.g. a column name.
        /// </summary>
        /// <param name="identifier">An identifier.</param>
        /// <returns>A quoted identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is <c>null</c>, empty or whitespace.</exception>
        public override string QuoteIdentifier(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            var isValid = identifier.All(IsValidIdentifierChar);
            if (!isValid)
                throw new ArgumentException("Identifier contains invalid characters ('\"', or '\\0').", nameof(identifier));

            return "\"" + identifier + "\"";
        }

        private static bool IsValidIdentifierChar(char identifierChar) => identifierChar != '"' && identifierChar != '\0';

        /// <summary>
        /// Quotes a qualified name.
        /// </summary>
        /// <param name="name">An object name.</param>
        /// <returns>A quoted name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
        public override string QuoteName(Identifier name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var pieces = new List<string>();

            if (name.Server != null)
                pieces.Add(QuoteIdentifier(name.Server));
            if (name.Database != null)
                pieces.Add(QuoteIdentifier(name.Database));
            if (name.Schema != null)
                pieces.Add(QuoteIdentifier(name.Schema));
            if (name.LocalName != null)
                pieces.Add(QuoteIdentifier(name.LocalName));

            return pieces.Join(".");
        }

        /// <summary>
        /// Retrieves the set of identifier defaults for the given database connection.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A set of identifier defaults.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public override Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return GetIdentifierDefaultsAsyncCore(connection, cancellationToken);
        }

        private static async Task<IIdentifierDefaults> GetIdentifierDefaultsAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
        {
            var hostInfoOption = connection.DbConnection.QueryFirstOrNone<GetIdentifierDefaultsQueryResult>(IdentifierDefaultsQuerySql, cancellationToken);
            var qualifiedServerName = await hostInfoOption
                .Bind(static dbHost => dbHost.ServerHost != null && dbHost.ServerSid != null
                    ? OptionAsync<GetIdentifierDefaultsQueryResult>.Some(dbHost)
                    : OptionAsync<GetIdentifierDefaultsQueryResult>.None
                )
                .MatchUnsafe(
                    static dbHost => dbHost.ServerHost + "/" + dbHost.ServerSid,
                    static () => (string?)null
                ).ConfigureAwait(false);
            var dbName = await hostInfoOption.MatchUnsafe(h => h.DatabaseName, () => null).ConfigureAwait(false);
            var defaultSchema = await hostInfoOption.MatchUnsafe(h => h.DefaultSchema, () => null).ConfigureAwait(false);

            return new IdentifierDefaults(qualifiedServerName, dbName, defaultSchema);
        }

        private static readonly string IdentifierDefaultsQuerySql = @$"
select
    SYS_CONTEXT('USERENV', 'SERVER_HOST') as ""{ nameof(GetIdentifierDefaultsQueryResult.ServerHost) }"",
    SYS_CONTEXT('USERENV', 'INSTANCE_NAME') as ""{ nameof(GetIdentifierDefaultsQueryResult.ServerSid) }"",
    SYS_CONTEXT('USERENV', 'DB_NAME') as ""{ nameof(GetIdentifierDefaultsQueryResult.DatabaseName) }"",
    SYS_CONTEXT('USERENV', 'CURRENT_USER') as ""{ nameof(GetIdentifierDefaultsQueryResult.DefaultSchema) }""
from DUAL";

        /// <summary>
        /// Gets the database display version. Usually a more user-friendly form of the database version.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A descriptive version.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public override Task<string> GetDatabaseDisplayVersionAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var versionInfoOption = connection.DbConnection.QueryFirstOrNone<GetDatabaseVersionQueryResult>(DatabaseVersionQuerySql, cancellationToken);
            return versionInfoOption.MatchUnsafe(
                static vInfo => vInfo.ProductName + vInfo.VersionNumber,
                static () => string.Empty
            );
        }

        /// <summary>
        /// Gets the database version.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A version.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public override Task<Version> GetDatabaseVersionAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var versionInfoOption = connection.DbConnection.QueryFirstOrNone<GetDatabaseVersionQueryResult>(DatabaseVersionQuerySql, cancellationToken);
            return versionInfoOption
                .Bind(static dbv => TryParseLongVersionString(dbv.VersionNumber).ToAsync())
                .MatchUnsafeAsync(
                    v => v,
                    static () => Task.FromResult(new Version(0, 0))
                );
        }

        private static Option<Version> TryParseLongVersionString(string? version)
        {
            if (version.IsNullOrWhiteSpace())
                return Option<Version>.None;

            var dotCount = version.Count(static c => c == '.');
            if (dotCount < 4)
            {
                return Version.TryParse(version, out var validVersion)
                    ? Option<Version>.Some(validVersion)
                    : Option<Version>.None;
            }

            // only take the first 4 version numbers and try again
            var versionStr = version
                .Split(new[] { '.' }, 5, StringSplitOptions.RemoveEmptyEntries)
                .Take(4)
                .Join(".");
            return Version.TryParse(versionStr, out var v)
                    ? Option<Version>.Some(v)
                    : Option<Version>.None;
        }

        private static readonly string DatabaseVersionQuerySql = @$"
select
    PRODUCT as ""{ nameof(GetDatabaseVersionQueryResult.ProductName) }"",
    VERSION as ""{ nameof(GetDatabaseVersionQueryResult.VersionNumber) }""
from PRODUCT_COMPONENT_VERSION
where PRODUCT like 'Oracle Database%'";

        /// <summary>
        /// Retrieves a relational database for the given dialect.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A relational database.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public override Task<IRelationalDatabase> GetRelationalDatabaseAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return GetRelationalDatabaseAsyncCore(connection, cancellationToken);
        }

        private static async Task<IRelationalDatabase> GetRelationalDatabaseAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
        {
            var identifierDefaults = await GetIdentifierDefaultsAsyncCore(connection, cancellationToken).ConfigureAwait(false);
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();
            return new OracleRelationalDatabase(connection, identifierDefaults, identifierResolver);
        }

        /// <summary>
        /// Retrieves a relational database comment provider for the given dialect.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A comment provider.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public override Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return GetRelationalDatabaseCommentProviderAsyncCore(connection, cancellationToken);
        }

        private static async Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
        {
            var identifierDefaults = await GetIdentifierDefaultsAsyncCore(connection, cancellationToken).ConfigureAwait(false);
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();
            return new OracleDatabaseCommentProvider(connection.DbConnection, identifierDefaults, identifierResolver);
        }

        /// <summary>
        /// Determines whether the given text is a reserved keyword.
        /// </summary>
        /// <param name="text">A piece of text.</param>
        /// <returns><c>true</c> if the given text is a reserved keyword; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>, empty or whitespace.</exception>
        public override bool IsReservedKeyword(string text)
        {
            if (text.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(text));

            return Keywords.Contains(text, StringComparer.Ordinal);
        }

        /// <summary>
        /// Gets a dependency provider for Oracle expressions.
        /// </summary>
        /// <returns>A dependency provider.</returns>
        public override IDependencyProvider GetDependencyProvider() => new OracleDependencyProvider();

        /// <summary>
        /// Gets a database column data type provider.
        /// </summary>
        /// <value>The type provider.</value>
        public override IDbTypeProvider TypeProvider => InnerTypeProvider;

        private static readonly IDbTypeProvider InnerTypeProvider = new OracleDbTypeProvider();

        // https://docs.oracle.com/database/121/SQLRF/ap_keywd.htm#SQLRF022
        private static readonly IEnumerable<string> Keywords = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ACCESS",
            "ADD",
            "ALL",
            "ALTER",
            "AND",
            "ANY",
            "AS",
            "ASC",
            "AUDIT",
            "BETWEEN",
            "BY",
            "CHAR",
            "CHECK",
            "CLUSTER",
            "COLUMN",
            "COLUMN_VALUE",
            "COMMENT",
            "COMPRESS",
            "CONNECT",
            "CREATE",
            "CURRENT",
            "DATE",
            "DECIMAL",
            "DEFAULT",
            "DELETE",
            "DESC",
            "DISTINCT",
            "DROP",
            "ELSE",
            "EXCLUSIVE",
            "EXISTS",
            "FILE",
            "FLOAT",
            "FOR",
            "FROM",
            "GRANT",
            "GROUP",
            "HAVING",
            "IDENTIFIED",
            "IMMEDIATE",
            "IN",
            "INCREMENT",
            "INDEX",
            "INITIAL",
            "INSERT",
            "INTEGER",
            "INTERSECT",
            "INTO",
            "IS",
            "LEVEL",
            "LIKE",
            "LOCK",
            "LONG",
            "MAXEXTENTS",
            "MINUS",
            "MLSLABEL",
            "MODE",
            "MODIFY",
            "NESTED_TABLE_ID",
            "NOAUDIT",
            "NOCOMPRESS",
            "NOT",
            "NOWAIT",
            "NULL",
            "NUMBER",
            "OF",
            "OFFLINE",
            "ON",
            "ONLINE",
            "OPTION",
            "OR",
            "ORDER",
            "PCTFREE",
            "PRIOR",
            "PUBLIC",
            "RAW",
            "RENAME",
            "RESOURCE",
            "REVOKE",
            "ROW",
            "ROWID",
            "ROWNUM",
            "ROWS",
            "SELECT",
            "SESSION",
            "SET",
            "SHARE",
            "SIZE",
            "SMALLINT",
            "START",
            "SUCCESSFUL",
            "SYNONYM",
            "SYSDATE",
            "TABLE",
            "THEN",
            "TO",
            "TRIGGER",
            "UID",
            "UNION",
            "UNIQUE",
            "UPDATE",
            "USER",
            "VALIDATE",
            "VALUES",
            "VARCHAR",
            "VARCHAR2",
            "VIEW",
            "WHENEVER",
            "WHERE",
            "WITH",

            // some extras are found in V$RESERVED_WORDS, complete collection here
            "!",
            "&",
            "(",
            ")",
            "*",
            "+",
            ",",
            "-",
            ".",
            "/",
            ":",
            "<",
            "=",
            ">",
            "@",
            "ALL",
            "ALTER",
            "AND",
            "ANY",
            "AS",
            "ASC",
            "BETWEEN",
            "BY",
            "CHAR",
            "CHECK",
            "CLUSTER",
            "COMPRESS",
            "CONNECT",
            "CREATE",
            "DATE",
            "DECIMAL",
            "DEFAULT",
            "DELETE",
            "DESC",
            "DISTINCT",
            "DROP",
            "ELSE",
            "EXCLUSIVE",
            "EXISTS",
            "FLOAT",
            "FOR",
            "FROM",
            "GRANT",
            "GROUP",
            "HAVING",
            "IDENTIFIED",
            "IN",
            "INDEX",
            "INSERT",
            "INTEGER",
            "INTERSECT",
            "INTO",
            "IS",
            "LIKE",
            "LOCK",
            "LONG",
            "MINUS",
            "MODE",
            "NOCOMPRESS",
            "NOT",
            "NOWAIT",
            "NULL",
            "NUMBER",
            "OF",
            "ON",
            "OPTION",
            "OR",
            "ORDER",
            "PCTFREE",
            "PRIOR",
            "PUBLIC",
            "RAW",
            "RENAME",
            "RESOURCE",
            "REVOKE",
            "SELECT",
            "SET",
            "SHARE",
            "SIZE",
            "SMALLINT",
            "START",
            "SYNONYM",
            "TABLE",
            "THEN",
            "TO",
            "TRIGGER",
            "UNION",
            "UNIQUE",
            "UPDATE",
            "VALUES",
            "VARCHAR",
            "VARCHAR2",
            "VIEW",
            "WHERE",
            "WITH",
            "[",
            "]",
            "^",
            "|"
        };
    }
}
