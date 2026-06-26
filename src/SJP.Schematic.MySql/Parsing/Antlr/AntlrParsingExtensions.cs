using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Parsing.Antlr;

/// <summary>
/// Helper routines shared by the ANTLR-based MySQL parsing consumers.
/// </summary>
internal static class AntlrParsingExtensions
{
    /// <summary>
    /// Determines whether a token may be used as an identifier in MySQL, i.e. it is a regular
    /// identifier, a back-tick or double-quoted (delimited) identifier, or a non-reserved keyword.
    /// </summary>
    /// <param name="token">A lexer token.</param>
    /// <returns><see langword="true" /> if the token may be used as an identifier; otherwise, <see langword="false" />.</returns>
    public static bool IsIdentifier(IToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        return MySqlIdentifierTokens.Types.Contains(token.Type);
    }

    /// <summary>
    /// Removes the surrounding delimiters from a delimited identifier, unescaping any doubled
    /// delimiters. Back-tick (<c>`name`</c>) and double-quoted (<c>"name"</c>) identifiers are
    /// supported; non-delimited identifiers are returned unchanged.
    /// </summary>
    /// <param name="identifier">An identifier token's text.</param>
    /// <returns>The bare identifier name.</returns>
    public static string UnquoteIdentifier(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        if (identifier.Length >= 2 && identifier[0] == '`' && identifier[^1] == '`')
            return identifier[1..^1].Replace("``", "`", StringComparison.Ordinal);

        // A double-quoted name is a delimited identifier under ANSI_QUOTES (how the lexer is configured).
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
