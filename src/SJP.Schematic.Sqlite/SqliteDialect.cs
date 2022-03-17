using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// A database dialect specific to SQLite.
/// </summary>
/// <seealso cref="DatabaseDialect" />
public class SqliteDialect : DatabaseDialect
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

        return GetDatabaseDisplayVersionAsyncCore(connection, cancellationToken);
    }

    private static async Task<string> GetDatabaseDisplayVersionAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var versionStr = await connection.DbConnection.ExecuteScalarAsync<string>(DatabaseDisplayVersionQuerySql, cancellationToken).ConfigureAwait(false);
        return "SQLite " + versionStr;
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

        return GetDatabaseVersionAsyncCore(connection, cancellationToken);
    }

    private static async Task<Version> GetDatabaseVersionAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var versionStr = await connection.DbConnection.ExecuteScalarAsync<string>(DatabaseDisplayVersionQuerySql, cancellationToken).ConfigureAwait(false);
        return Version.Parse(versionStr);
    }

    private const string DatabaseDisplayVersionQuerySql = "select sqlite_version()";

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

        return GetRelationalDatabaseAsyncCore(connection);
    }

    private static async Task<IRelationalDatabase> GetRelationalDatabaseAsyncCore(ISchematicConnection connection)
    {
        var identifierDefaults = await GetIdentifierDefaultsAsyncCore().ConfigureAwait(false);
        return new SqliteRelationalDatabase(connection, identifierDefaults, new ConnectionPragma(connection));
    }

    /// <summary>
    /// Retrieves a relational database comment provider for the given dialect.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A comment provider.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
    public override async Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
    {
        var identifierDefaults = await GetIdentifierDefaultsAsyncCore().ConfigureAwait(false);
        return new EmptyRelationalDatabaseCommentProvider(identifierDefaults);
    }

    /// <summary>
    /// Gets a dependency provider that retrieves dependencies for SQLite statements.
    /// </summary>
    /// <returns>A dependency provider.</returns>
    public override IDependencyProvider GetDependencyProvider() => new SqliteDependencyProvider();

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

        if (name.Schema != null)
        {
            return QuoteIdentifier(name.Schema)
                + "."
                + QuoteIdentifier(name.LocalName);
        }

        return QuoteIdentifier(name.LocalName);
    }

    /// <summary>
    /// Gets a database column data type provider.
    /// </summary>
    /// <value>The type provider.</value>
    public override IDbTypeProvider TypeProvider => InnerTypeProvider;

    private static readonly IDbTypeProvider InnerTypeProvider = new SqliteDbTypeProvider();
}
