using System.Collections.Generic;
using Antlr4.Runtime;

namespace SJP.Schematic.Sqlite.Parsing.Antlr;

/// <summary>
/// Helpers for lexing SQLite SQL into ANTLR tokens.
/// </summary>
internal static class SqliteLexing
{
    /// <summary>
    /// Lexes SQL text and returns the significant tokens, i.e. those on the default channel
    /// (whitespace and comments are emitted on the hidden channel) excluding the end-of-file
    /// marker. The lexer is configured to throw on the first lexical error.
    /// </summary>
    /// <param name="sql">SQL text to lex.</param>
    /// <returns>The significant tokens in source order.</returns>
    /// <exception cref="SqliteSyntaxErrorException">The SQL could not be lexed.</exception>
    public static IReadOnlyList<IToken> GetSignificantTokens(string sql)
    {
        var inputStream = new AntlrInputStream(sql);
        var lexer = new SQLiteLexer(inputStream);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(ThrowingErrorListener.Instance);

        // Pull tokens straight from the source rather than buffering them in a token stream first;
        // hidden tokens are neither allocated nor returned.
        var tokenSource = new SignificantTokenSource(lexer);
        var tokens = new List<IToken>();
        for (var token = tokenSource.NextToken(); token.Type != TokenConstants.EOF; token = tokenSource.NextToken())
            tokens.Add(token);

        return tokens;
    }
}
