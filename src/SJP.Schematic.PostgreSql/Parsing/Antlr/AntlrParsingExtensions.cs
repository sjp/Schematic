using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Parsing.Antlr;

/// <summary>
/// Helper routines shared by the ANTLR-based PostgreSQL parsing consumers.
/// </summary>
internal static class AntlrParsingExtensions
{
    /// <summary>
    /// Determines whether a token may be used as an identifier in PostgreSQL, i.e. it is a regular
    /// identifier, a (Unicode-)quoted delimited identifier, or a non-reserved keyword.
    /// </summary>
    /// <param name="token">A lexer token.</param>
    /// <returns><see langword="true" /> if the token may be used as an identifier; otherwise, <see langword="false" />.</returns>
    public static bool IsIdentifier(IToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        return PostgreSqlIdentifierTokens.Types.Contains(token.Type);
    }

    /// <summary>
    /// Removes the surrounding delimiters from a delimited identifier, unescaping any doubled
    /// quotes. Double-quoted (<c>"name"</c>) and Unicode-quoted (<c>U&amp;"name"</c>) identifiers are
    /// supported; non-delimited identifiers are returned unchanged.
    /// </summary>
    /// <param name="identifier">An identifier token's text.</param>
    /// <returns>The bare identifier name.</returns>
    public static string UnquoteIdentifier(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        // A Unicode-quoted identifier is introduced by a U& prefix, e.g. U&"d\0061ta".
        if (identifier.Length >= 3
            && (identifier[0] == 'U' || identifier[0] == 'u')
            && identifier[1] == '&'
            && identifier[2] == '"'
            && identifier[^1] == '"')
            return identifier[3..^1].Replace("\"\"", "\"", StringComparison.Ordinal);

        if (identifier.Length >= 2 && identifier[0] == '"' && identifier[^1] == '"')
            return identifier[1..^1].Replace("\"\"", "\"", StringComparison.Ordinal);

        return identifier;
    }

    /// <summary>
    /// Builds an <see cref="Identifier"/> from the parts of a qualified name.
    /// </summary>
    /// <param name="parts">The ordered parts of a qualified name, e.g. <c>schema</c>, <c>table</c>.</param>
    /// <returns>An identifier composed of the (up to four) right-most parts.</returns>
    public static Identifier BuildIdentifier(IReadOnlyList<string> parts)
    {
        ArgumentNullException.ThrowIfNull(parts);

        // Identifier supports at most four levels (server.database.schema.local); keep the
        // right-most parts if somehow more were found.
        var relevant = parts.Count > 4
            ? parts.Skip(parts.Count - 4).ToList()
            : parts;

        return relevant.Count switch
        {
            1 => Identifier.CreateQualifiedIdentifier(relevant[0]),
            2 => Identifier.CreateQualifiedIdentifier(relevant[0], relevant[1]),
            3 => Identifier.CreateQualifiedIdentifier(relevant[0], relevant[1], relevant[2]),
            _ => Identifier.CreateQualifiedIdentifier(relevant[0], relevant[1], relevant[2], relevant[3]),
        };
    }
}
