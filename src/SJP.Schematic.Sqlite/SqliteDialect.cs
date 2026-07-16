using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// A database dialect specific to SQLite.
/// </summary>
/// <seealso cref="DatabaseDialect" />
public class SqliteDialect : DatabaseDialect
{
    /// <summary>
    /// Gets a dependency provider that retrieves dependencies for SQLite statements.
    /// </summary>
    /// <returns>A dependency provider.</returns>
    public override IDependencyProvider GetDependencyProvider() => new SqliteDependencyProvider();

    /// <summary>
    /// Determines whether the given text is a reserved keyword.
    /// </summary>
    /// <param name="text">A piece of text.</param>
    /// <returns><see langword="true" /> if the given text is a reserved keyword; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null" />, empty or whitespace.</exception>
    public override bool IsReservedKeyword(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

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
        "WITHOUT",
    };

    /// <summary>
    /// Quotes a qualified name.
    /// </summary>
    /// <param name="name">An object name.</param>
    /// <returns>A quoted name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />.</exception>
    public override string QuoteName(Identifier name)
    {
        ArgumentNullException.ThrowIfNull(name);

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