using System.Collections.Generic;
using Antlr4.Runtime;

namespace SJP.Schematic.Oracle.Parsing.Antlr;

/// <summary>
/// Helpers for lexing Oracle/PL-SQL text into ANTLR tokens.
/// </summary>
internal static class OracleLexing
{
    /// <summary>
    /// Lexes SQL text and returns the significant tokens, i.e. those on the default channel
    /// (whitespace and comments are emitted on the hidden channel) excluding the end-of-file
    /// marker. The lexer is configured to throw on the first lexical error.
    /// </summary>
    /// <param name="sql">SQL text to lex.</param>
    /// <returns>The significant tokens in source order.</returns>
    /// <exception cref="OracleSyntaxErrorException">The SQL could not be lexed.</exception>
    public static IReadOnlyList<IToken> GetSignificantTokens(string sql)
        => Lex(sql, ThrowingErrorListener.Instance);

    private static IReadOnlyList<IToken> Lex(string sql, IAntlrErrorListener<int> errorListener)
    {
        var inputStream = new AntlrInputStream(sql);
        var lexer = new PlSqlLexer(inputStream);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(errorListener);

        // Pull tokens straight from the lexer rather than buffering every token, hidden ones included,
        // in a token stream first.
        var tokens = new List<IToken>();
        for (var token = lexer.NextToken(); token.Type != TokenConstants.EOF; token = lexer.NextToken())
        {
            if (token.Channel == Lexer.DefaultTokenChannel)
                tokens.Add(token);
        }

        return tokens;
    }
}
