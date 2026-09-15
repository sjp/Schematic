using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;

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

    // Besides the keyword rules (all named *_SYMBOL), these are the grammar rules whose semantic predicates
    // are constant for every lexer created here. Keyword predicates test the server version and the
    // quoted text predicates test for the NO_BACKSLASH_ESCAPES SQL mode, and both are fixed by the
    // MySQLLexerBase constructor. The remaining predicates are left as they are: the version comment
    // ones read the matched text or lexer state, and dollar-quoted text is rare.
    private static readonly string[] ConstantPredicateRules =
    [
        "GRAMMAR_SELECTOR_DERIVED_EXPR",
        "SINGLE_QUOTED_TEXT",
        "DOUBLE_QUOTED_TEXT",
    ];

    static MySqlLexing()
    {
        // Must run before any lexer uses the shared ATN, so the shared DFA cache only ever sees the
        // patched ATN. Every lexer is created by this class, so the type initializer guarantees that.
        RemoveConstantPredicates(MySQLLexer._ATN);
    }

    /// <summary>
    /// Replaces the constant semantic predicates in the lexer's shared ATN with their outcome: a
    /// predicate that is always true becomes an epsilon transition to the same target, and one that is
    /// always false is removed.
    /// </summary>
    /// <param name="atn">The lexer ATN to patch in place.</param>
    /// <remarks>
    /// The ANTLR lexer never caches a DFA edge whose closure passed through a semantic predicate, so
    /// without this every character inside a quoted string, and the last character of every token that
    /// begins with a version-gated keyword (e.g. <c>description</c> or <c>offset</c>), would be simulated
    /// on the ATN again on every lex. The patched ATN accepts exactly the same tokens.
    /// </remarks>
    private static void RemoveConstantPredicates(ATN atn)
    {
        var probe = new MySQLLexer(new AntlrInputStream(string.Empty));

        foreach (var state in atn.states)
        {
            if (state == null)
                continue;

            // Iterate backwards so removing a transition does not shift the ones still to be visited.
            for (var i = state.NumberOfTransitions - 1; i >= 0; i--)
            {
                if (state.Transition(i) is not PredicateTransition predicate || !HasConstantPredicates(predicate.ruleIndex))
                    continue;

                if (probe.Sempred(null, predicate.ruleIndex, predicate.predIndex))
                    state.SetTransition(i, new EpsilonTransition(predicate.target));
                else
                    state.RemoveTransition(i);
            }
        }
    }

    private static bool HasConstantPredicates(int ruleIndex)
    {
        var ruleName = MySQLLexer.ruleNames[ruleIndex];
        return ruleName.EndsWith("_SYMBOL", StringComparison.Ordinal)
            || ConstantPredicateRules.Contains(ruleName, StringComparer.Ordinal);
    }

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
