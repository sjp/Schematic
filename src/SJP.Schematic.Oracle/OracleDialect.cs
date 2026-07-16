using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle;

/// <summary>
/// A database dialect specific to Oracle.
/// </summary>
/// <seealso cref="DatabaseDialect" />
public class OracleDialect : DatabaseDialect
{
    /// <summary>
    /// Quotes a string identifier, e.g. a column name.
    /// </summary>
    /// <param name="identifier">An identifier.</param>
    /// <returns>A quoted identifier.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is <see langword="null" />, empty or whitespace.</exception>
    public override string QuoteIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

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
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />.</exception>
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
    /// Determines whether the given text is a reserved keyword.
    /// </summary>
    /// <param name="text">A piece of text.</param>
    /// <returns><see langword="true" /> if the given text is a reserved keyword; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null" />, empty or whitespace.</exception>
    public override bool IsReservedKeyword(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

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
        "|",
    };
}