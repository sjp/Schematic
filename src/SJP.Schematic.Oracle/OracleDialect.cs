using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Oracle.ManagedDataAccess.Client;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Query;

namespace SJP.Schematic.Oracle
{
    public class OracleDialect : DatabaseDialect<OracleDialect>
    {
        public override IDbConnection CreateConnection(string connectionString)
        {
            if (connectionString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(connectionString));

            var connection = new OracleConnection(connectionString);
            connection.Open();
            return connection;
        }

        public override Task<IDbConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connectionString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(connectionString));

            return CreateConnectionAsyncCore(connectionString, cancellationToken);
        }

        private static async Task<IDbConnection> CreateConnectionAsyncCore(string connectionString, CancellationToken cancellationToken)
        {
            var connection = new OracleConnection(connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }

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

        public override IIdentifierDefaults GetIdentifierDefaults(IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var hostInfoOption = connection.QueryFirstOrNone<DatabaseHost>(IdentifierDefaultsQuerySql);
            var qualifiedServerName = hostInfoOption.MatchUnsafe(
                dbHost => dbHost.ServerHost + "/" + dbHost.ServerSid,
                () => null
            );
            var dbName = hostInfoOption.MatchUnsafe(h => h.DatabaseName, () => null);
            var defaultSchema = hostInfoOption.MatchUnsafe(h => h.DefaultSchema, () => null);

            return new IdentifierDefaultsBuilder()
                .WithServer(qualifiedServerName)
                .WithDatabase(dbName)
                .WithSchema(defaultSchema)
                .Build();
        }

        public override Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(IDbConnection connection, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return GetIdentifierDefaultsAsyncCore(connection, cancellationToken);
        }

        private static async Task<IIdentifierDefaults> GetIdentifierDefaultsAsyncCore(IDbConnection connection, CancellationToken cancellationToken)
        {
            var hostInfoOption = connection.QueryFirstOrNoneAsync<DatabaseHost>(IdentifierDefaultsQuerySql, cancellationToken);
            var qualifiedServerName = await hostInfoOption.MatchUnsafe(
                dbHost => dbHost.ServerHost + "/" + dbHost.ServerSid,
                () => null
            ).ConfigureAwait(false);
            var dbName = await hostInfoOption.MatchUnsafe(h => h.DatabaseName, () => null).ConfigureAwait(false);
            var defaultSchema = await hostInfoOption.MatchUnsafe(h => h.DefaultSchema, () => null).ConfigureAwait(false);

            return new IdentifierDefaultsBuilder()
                .WithServer(qualifiedServerName)
                .WithDatabase(dbName)
                .WithSchema(defaultSchema)
                .Build();
        }

        private const string IdentifierDefaultsQuerySql = @"
select
    SYS_CONTEXT('USERENV', 'SERVER_HOST') as ServerHost,
    SYS_CONTEXT('USERENV', 'INSTANCE_NAME') as ServerSid,
    SYS_CONTEXT('USERENV', 'DB_NAME') as DatabaseName,
    SYS_CONTEXT('USERENV', 'CURRENT_USER') as DefaultSchema
from DUAL";

        public override string GetDatabaseDisplayVersion(IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var versionInfoOption = connection.QueryFirstOrNone<DatabaseVersion>(DatabaseVersionQuerySql);
            return versionInfoOption.MatchUnsafe(
                vInfo => vInfo.ProductName + vInfo.VersionNumber,
                () => null
            );
        }

        public override Task<string> GetDatabaseDisplayVersionAsync(IDbConnection connection, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var versionInfoOption = connection.QueryFirstOrNoneAsync<DatabaseVersion>(DatabaseVersionQuerySql, cancellationToken);
            return versionInfoOption.MatchUnsafe(
                vInfo => vInfo.ProductName + vInfo.VersionNumber,
                () => null
            );
        }

        public override Version GetDatabaseVersion(IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var versionInfoOption = connection.QueryFirstOrNone<DatabaseVersion>(DatabaseVersionQuerySql);
            return versionInfoOption
                .Bind(dbv => TryParseLongVersionString(dbv.VersionNumber))
                .MatchUnsafe(
                    v => v,
                    () => null
                );
        }

        public override Task<Version> GetDatabaseVersionAsync(IDbConnection connection, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var versionInfoOption = connection.QueryFirstOrNoneAsync<DatabaseVersion>(DatabaseVersionQuerySql, cancellationToken);
            return versionInfoOption
                .Bind(dbv => TryParseLongVersionString(dbv.VersionNumber).ToAsync())
                .MatchUnsafeAsync(
                    v => v,
                    () => Task.FromResult<Version>(null)
                );
        }

        private static Option<Version> TryParseLongVersionString(string version)
        {
            if (version.IsNullOrWhiteSpace())
                return Option<Version>.None;

            var dotCount = version.Count(c => c == '.');
            if (dotCount < 4)
            {
                return Version.TryParse(version, out var validVersion)
                    ? Option<Version>.Some(validVersion)
                    : Option<Version>.None;
            }

            // only take the first 4 version numbers and try again
            var versionStr = version
                .Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                .Take(4)
                .Join(".");
            return Version.TryParse(versionStr, out var v)
                    ? Option<Version>.Some(v)
                    : Option<Version>.None;
        }

        private const string DatabaseVersionQuerySql = @"
select
    PRODUCT as ProductName,
    VERSION as VersionNumber
from PRODUCT_COMPONENT_VERSION
where PRODUCT like 'Oracle Database%'";

        public override bool IsReservedKeyword(string text)
        {
            if (text.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(text));

            return _keywords.Contains(text);
        }

        public override IDbTypeProvider TypeProvider => _typeProvider;

        private readonly static IDbTypeProvider _typeProvider = new OracleDbTypeProvider();

        // https://docs.oracle.com/database/121/SQLRF/ap_keywd.htm#SQLRF022
        private readonly static IEnumerable<string> _keywords = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase)
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
