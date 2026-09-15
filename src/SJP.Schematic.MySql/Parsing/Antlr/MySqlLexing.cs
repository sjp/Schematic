using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace SJP.Schematic.MySql.Parsing.Antlr;

/// <summary>
/// Helpers for lexing MySQL text into ANTLR tokens.
/// </summary>
internal static class MySqlLexing
{
    // The lexer only emits UNDERSCORE_CHARSET for "_name" text when the name, underscore included,
    // is in its character set collection; otherwise the text is lexed as an ordinary identifier.
    // These are the character sets that MySQL 8 and MariaDB accept as string literal introducers
    // (e.g. _utf8mb4'text'), including the utf8 alias and the internal filename set. Introducers
    // are case-insensitive. The lexer only ever reads the set, so one instance is shared.
    private static readonly HashSet<string> CharacterSetIntroducers = new(StringComparer.OrdinalIgnoreCase)
    {
        "_armscii8",
        "_ascii",
        "_big5",
        "_binary",
        "_cp1250",
        "_cp1251",
        "_cp1256",
        "_cp1257",
        "_cp850",
        "_cp852",
        "_cp866",
        "_cp932",
        "_dec8",
        "_eucjpms",
        "_euckr",
        "_filename",
        "_gb18030",
        "_gb2312",
        "_gbk",
        "_geostd8",
        "_greek",
        "_hebrew",
        "_hp8",
        "_keybcs2",
        "_koi8r",
        "_koi8u",
        "_latin1",
        "_latin2",
        "_latin5",
        "_latin7",
        "_macce",
        "_macroman",
        "_sjis",
        "_swe7",
        "_tis620",
        "_ucs2",
        "_ujis",
        "_utf16",
        "_utf16le",
        "_utf32",
        "_utf8",
        "_utf8mb3",
        "_utf8mb4",
    };

    /// <summary>
    /// Lexes SQL text and returns the significant tokens, i.e. those on the default channel
    /// (whitespace and comments are emitted on the hidden channel) excluding the end-of-file
    /// marker. The lexer is configured to throw on the first lexical error.
    /// </summary>
    /// <param name="sql">SQL text to lex.</param>
    /// <returns>The significant tokens in source order.</returns>
    /// <exception cref="MySqlSyntaxErrorException">The SQL could not be lexed.</exception>
    public static IReadOnlyList<IToken> GetSignificantTokens(string sql)
        => Lex(sql, ThrowingErrorListener.Instance);

    /// <summary>
    /// Lexes SQL text and returns the significant tokens on a best-effort basis. Unlike
    /// <see cref="GetSignificantTokens(string)"/> this never throws on a lexical error; the lexer
    /// instead skips offending input and continues.
    /// </summary>
    /// <param name="sql">SQL text to lex.</param>
    /// <returns>The significant tokens in source order.</returns>
    public static IReadOnlyList<IToken> GetSignificantTokensSafe(string sql)
        => Lex(sql, errorListener: null);

    private static IReadOnlyList<IToken> Lex(string sql, IAntlrErrorListener<int>? errorListener)
    {
        var inputStream = new AntlrInputStream(sql);
        var lexer = new MySQLLexer(inputStream) { charSets = CharacterSetIntroducers };
        lexer.RemoveErrorListeners();
        if (errorListener != null)
            lexer.AddErrorListener(errorListener);

        var tokenStream = new CommonTokenStream(lexer);
        tokenStream.Fill();

        return tokenStream.GetTokens()
            .Where(static t => t.Channel == Lexer.DefaultTokenChannel && t.Type != TokenConstants.EOF)
            .ToList();
    }
}
