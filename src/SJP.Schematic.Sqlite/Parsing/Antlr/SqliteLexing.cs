using System.Collections.Generic;
using System.Linq;
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

        var tokenStream = new CommonTokenStream(lexer);
        tokenStream.Fill();

        return tokenStream.GetTokens()
            .Where(static t => t.Channel == Lexer.DefaultTokenChannel && t.Type != TokenConstants.EOF)
            .ToList();
    }
}
