using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Comments;
using SJP.Schematic.SqlServer.Queries;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// A database dialect specific to SQL Server.
/// </summary>
/// <seealso cref="DatabaseDialect" />
public class SqlServerDialect : DatabaseDialect
{
    /// <summary>
    /// Retrieves the set of identifier defaults for the given database connection.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A set of identifier defaults.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
    public override Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return GetIdentifierDefaultsAsyncCore(connection, cancellationToken);
    }

    private static async Task<IIdentifierDefaults> GetIdentifierDefaultsAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        return await connection.DbConnection.QuerySingleAsync<GetIdentifierDefaults.Result>(GetIdentifierDefaults.Sql, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the database display version. Usually a more user-friendly form of the database version.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A descriptive version.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
    public override Task<string> GetDatabaseDisplayVersionAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return connection.DbConnection.ExecuteScalarAsync<string>(DatabaseDisplayVersionQuerySql, cancellationToken);
    }

    private const string DatabaseDisplayVersionQuerySql = "select @@version as DatabaseVersion";

    /// <summary>
    /// Gets the database version.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A version.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
    public override Task<Version> GetDatabaseVersionAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return GetDatabaseVersionAsyncCore(connection, cancellationToken);
    }

    private static async Task<Version> GetDatabaseVersionAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var versionStr = await connection.DbConnection.ExecuteScalarAsync<string>(GetDatabaseVersion.Sql, cancellationToken).ConfigureAwait(false);
        return Version.Parse(versionStr);
    }

    /// <summary>
    /// Retrieves a relational database for the given dialect.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
    public override Task<IRelationalDatabase> GetRelationalDatabaseAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return GetRelationalDatabaseAsyncCore(connection, cancellationToken);
    }

    private static async Task<IRelationalDatabase> GetRelationalDatabaseAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var identifierDefaults = await GetIdentifierDefaultsAsyncCore(connection, cancellationToken).ConfigureAwait(false);
        return new SqlServerRelationalDatabase(connection, identifierDefaults);
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
        ArgumentNullException.ThrowIfNull(connection);

        return GetRelationalDatabaseCommentProviderAsyncCore(connection, cancellationToken);
    }

    private static async Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var identifierDefaults = await GetIdentifierDefaultsAsyncCore(connection, cancellationToken).ConfigureAwait(false);
        return new SqlServerDatabaseCommentProvider(connection.DbConnection, identifierDefaults);
    }

    /// <summary>
    /// Gets a dependency provider for SQL Server expressions.
    /// </summary>
    /// <returns>A dependency provider.</returns>
    public override IDependencyProvider GetDependencyProvider() => new SqlServerDependencyProvider();

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

        return Keywords.Contains(text, StringComparer.OrdinalIgnoreCase);
    }

    // https://docs.microsoft.com/en-us/sql/t-sql/language-elements/reserved-keywords-transact-sql
    private static readonly IEnumerable<string> Keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "ABSOLUTE",
        "ACTION",
        "ADA",
        "ADD",
        "ALL",
        "ALLOCATE",
        "ALTER",
        "AND",
        "ANY",
        "ARE",
        "AS",
        "ASC",
        "ASSERTION",
        "AT",
        "AUTHORIZATION",
        "AVG",
        "BACKUP",
        "BEGIN",
        "BETWEEN",
        "BIT",
        "BIT_LENGTH",
        "BOTH",
        "BREAK",
        "BROWSE",
        "BULK",
        "BY",
        "CASCADE",
        "CASCADED",
        "CASE",
        "CAST",
        "CATALOG",
        "CHAR",
        "CHAR_LENGTH",
        "CHARACTER",
        "CHARACTER_LENGTH",
        "CHECK",
        "CHECKPOINT",
        "CLOSE",
        "CLUSTERED",
        "COALESCE",
        "COLLATE",
        "COLLATION",
        "COLUMN",
        "COMMIT",
        "COMPUTE",
        "CONNECT",
        "CONNECTION",
        "CONSTRAINT",
        "CONSTRAINTS",
        "CONTAINS",
        "CONTAINSTABLE",
        "CONTINUE",
        "CONVERT",
        "CORRESPONDING",
        "COUNT",
        "CREATE",
        "CROSS",
        "CURRENT",
        "CURRENT_DATE",
        "CURRENT_TIME",
        "CURRENT_TIMESTAMP",
        "CURRENT_USER",
        "CURSOR",
        "DATABASE",
        "DATE",
        "DAY",
        "DBCC",
        "DEALLOCATE",
        "DEC",
        "DECIMAL",
        "DECLARE",
        "DEFAULT",
        "DEFERRABLE",
        "DEFERRED",
        "DELETE",
        "DENY",
        "DESC",
        "DESCRIBE",
        "DESCRIPTOR",
        "DIAGNOSTICS",
        "DISCONNECT",
        "DISK",
        "DISTINCT",
        "DISTRIBUTED",
        "DOMAIN",
        "DOUBLE",
        "DROP",
        "DUMP",
        "ELSE",
        "END",
        "END-EXEC",
        "ERRLVL",
        "ESCAPE",
        "EXCEPT",
        "EXCEPTION",
        "EXEC",
        "EXECUTE",
        "EXISTS",
        "EXIT",
        "EXTERNAL",
        "EXTRACT",
        "FALSE",
        "FETCH",
        "FILE",
        "FILLFACTOR",
        "FIRST",
        "FLOAT",
        "FOR",
        "FOREIGN",
        "FORTRAN",
        "FOUND",
        "FREETEXT",
        "FREETEXTTABLE",
        "FROM",
        "FULL",
        "FUNCTION",
        "GET",
        "GLOBAL",
        "GO",
        "GOTO",
        "GRANT",
        "GROUP",
        "HAVING",
        "HOLDLOCK",
        "HOUR",
        "IDENTITY",
        "IDENTITY_INSERT",
        "IDENTITYCOL",
        "IF",
        "IMMEDIATE",
        "IN",
        "INCLUDE",
        "INDEX",
        "INDICATOR",
        "INITIALLY",
        "INNER",
        "INPUT",
        "INSENSITIVE",
        "INSERT",
        "INT",
        "INTEGER",
        "INTERSECT",
        "INTERVAL",
        "INTO",
        "IS",
        "ISOLATION",
        "JOIN",
        "KEY",
        "KILL",
        "LANGUAGE",
        "LAST",
        "LEADING",
        "LEFT",
        "LEVEL",
        "LIKE",
        "LINENO",
        "LOAD",
        "LOCAL",
        "LOWER",
        "MATCH",
        "MAX",
        "MERGE",
        "MIN",
        "MINUTE",
        "MODULE",
        "MONTH",
        "NAMES",
        "NATIONAL",
        "NATURAL",
        "NCHAR",
        "NEXT",
        "NO",
        "NOCHECK",
        "NONCLUSTERED",
        "NONE",
        "NOT",
        "NULL",
        "NULLIF",
        "NUMERIC",
        "OCTET_LENGTH",
        "OF",
        "OFF",
        "OFFSETS",
        "ON",
        "ONLY",
        "OPEN",
        "OPENDATASOURCE",
        "OPENQUERY",
        "OPENROWSET",
        "OPENXML",
        "OPTION",
        "OR",
        "ORDER",
        "OUTER",
        "OUTPUT",
        "OVER",
        "OVERLAPS",
        "PAD",
        "PARTIAL",
        "PASCAL",
        "PERCENT",
        "PIVOT",
        "PLAN",
        "POSITION",
        "PRECISION",
        "PREPARE",
        "PRESERVE",
        "PRIMARY",
        "PRINT",
        "PRIOR",
        "PRIVILEGES",
        "PROC",
        "PROCEDURE",
        "PUBLIC",
        "RAISERROR",
        "READ",
        "READTEXT",
        "REAL",
        "RECONFIGURE",
        "REFERENCES",
        "RELATIVE",
        "REPLICATION",
        "RESTORE",
        "RESTRICT",
        "RETURN",
        "REVERT",
        "REVOKE",
        "RIGHT",
        "ROLLBACK",
        "ROWCOUNT",
        "ROWGUIDCOL",
        "ROWS",
        "RULE",
        "SAVE",
        "SCHEMA",
        "SCROLL",
        "SECOND",
        "SECTION",
        "SECURITYAUDIT",
        "SELECT",
        "SEMANTICKEYPHRASETABLE",
        "SEMANTICSIMILARITYDETAILSTABLE",
        "SEMANTICSIMILARITYTABLE",
        "SESSION",
        "SESSION_USER",
        "SET",
        "SETUSER",
        "SHUTDOWN",
        "SIZE",
        "SMALLINT",
        "SOME",
        "SPACE",
        "SQL",
        "SQLCA",
        "SQLCODE",
        "SQLERROR",
        "SQLSTATE",
        "SQLWARNING",
        "STATISTICS",
        "SUBSTRING",
        "SUM",
        "SYSTEM_USER",
        "TABLE",
        "TABLESAMPLE",
        "TEMPORARY",
        "TEXTSIZE",
        "THEN",
        "TIME",
        "TIMESTAMP",
        "TIMEZONE_HOUR",
        "TIMEZONE_MINUTE",
        "TO",
        "TOP",
        "TRAILING",
        "TRAN",
        "TRANSACTION",
        "TRANSLATE",
        "TRANSLATION",
        "TRIGGER",
        "TRIM",
        "TRUE",
        "TRUNCATE",
        "TRY_CONVERT",
        "TSEQUAL",
        "UNION",
        "UNIQUE",
        "UNKNOWN",
        "UNPIVOT",
        "UPDATE",
        "UPDATETEXT",
        "UPPER",
        "USAGE",
        "USE",
        "USER",
        "USING",
        "VALUE",
        "VALUES",
        "VARCHAR",
        "VARYING",
        "VIEW",
        "WAITFOR",
        "WHEN",
        "WHENEVER",
        "WHERE",
        "WHILE",
        "WITH",
        "WITHIN GROUP",
        "WORK",
        "WRITE",
        "WRITETEXT",
        "YEAR",
        "ZONE"
    };

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

        return $"[{ identifier.Replace("]", "]]", StringComparison.Ordinal) }]";
    }

    /// <summary>
    /// Quotes a qualified name.
    /// </summary>
    /// <param name="name">An object name.</param>
    /// <returns>A quoted name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
    public override string QuoteName(Identifier name)
    {
        ArgumentNullException.ThrowIfNull(name);

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
    /// Gets a database column data type provider.
    /// </summary>
    /// <value>The type provider.</value>
    public override IDbTypeProvider TypeProvider => InnerTypeProvider;

    private static readonly IDbTypeProvider InnerTypeProvider = new SqlServerDbTypeProvider();
}