using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace SJP.Schematic.Sqlite.Parsing.Antlr;

/// <summary>
/// Helper methods for translating ANTLR parse trees into Schematic's parsed model types.
/// </summary>
internal static class AntlrParsingExtensions
{
    /// <summary>
    /// Recovers the exact original source text spanned by a parse tree node, preserving the
    /// inner whitespace of the source. This is used in preference to <c>GetText()</c>, which
    /// concatenates token text without whitespace and discards comments.
    /// </summary>
    public static string OriginalText(this ParserRuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var interval = Interval.Of(context.Start.StartIndex, context.Stop.StopIndex);
        return context.Start.InputStream.GetText(interval);
    }

    /// <summary>
    /// Recovers the exact original source text spanning from an opening terminal to a closing
    /// terminal (inclusive), e.g. the parenthesized body of a <c>CHECK (...)</c> constraint.
    /// </summary>
    public static string OriginalText(ITerminalNode open, ITerminalNode close)
    {
        ArgumentNullException.ThrowIfNull(open);
        ArgumentNullException.ThrowIfNull(close);

        var interval = Interval.Of(open.Symbol.StartIndex, close.Symbol.StopIndex);
        return open.Symbol.InputStream.GetText(interval);
    }

    /// <summary>
    /// Removes the wrapping quote/bracket characters from a SQLite identifier, if present.
    /// Handles double-quoted, backtick-quoted and bracket-quoted identifiers.
    /// </summary>
    public static string UnquoteIdentifier(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        if (identifier.Length < 2)
            return identifier;

        if (identifier.StartsWith('"'))
            return TrimWrappingChars(identifier).Replace("\"\"", "\"", StringComparison.Ordinal);
        if (identifier.StartsWith('['))
            return TrimWrappingChars(identifier).Replace("]]", "]", StringComparison.Ordinal);
        if (identifier.StartsWith('`'))
            return TrimWrappingChars(identifier).Replace("``", "`", StringComparison.Ordinal);

        return identifier;
    }

    /// <summary>
    /// Maps a SQLite collation name to its <see cref="SqliteCollation"/> enum value.
    /// Unknown collations map to <see cref="SqliteCollation.None"/>.
    /// </summary>
    public static SqliteCollation MapCollation(string collationName)
    {
        var unquoted = UnquoteIdentifier(collationName);
        if (string.Equals(unquoted, "BINARY", StringComparison.OrdinalIgnoreCase))
            return SqliteCollation.Binary;
        if (string.Equals(unquoted, "NOCASE", StringComparison.OrdinalIgnoreCase))
            return SqliteCollation.NoCase;
        if (string.Equals(unquoted, "RTRIM", StringComparison.OrdinalIgnoreCase))
            return SqliteCollation.Rtrim;

        return SqliteCollation.None;
    }

    /// <summary>
    /// Determines whether an indexed-column expression is a simple, unqualified column
    /// reference and, if so, returns its (unquoted) name. Returns <see langword="null"/>
    /// for compound expressions (e.g. <c>a + b</c>) or qualified references (e.g. <c>t.a</c>).
    /// </summary>
    public static string? TryGetSimpleColumnName(SQLiteParser.ExprContext expr)
    {
        IParseTree node = expr;
        while (true)
        {
            if (node is SQLiteParser.Column_name_excluding_stringContext columnName)
                return UnquoteIdentifier(columnName.GetText());
            if (node.ChildCount != 1)
                return null;
            node = node.GetChild(0);
        }
    }

    private static string TrimWrappingChars(string input) => input[1..^1];
}
