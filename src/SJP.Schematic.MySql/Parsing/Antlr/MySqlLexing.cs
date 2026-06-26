using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace SJP.Schematic.MySql.Parsing.Antlr;

/// <summary>
/// Helpers for lexing MySQL text into ANTLR tokens.
/// </summary>
internal static class MySqlLexing
{
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
        var lexer = new MySQLLexer(inputStream);
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
