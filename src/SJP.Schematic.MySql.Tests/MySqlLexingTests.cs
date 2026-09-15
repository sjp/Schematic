using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using NUnit.Framework;
using SJP.Schematic.MySql.Parsing.Antlr;

namespace SJP.Schematic.MySql.Tests;

[TestFixture]
internal static class MySqlLexingTests
{
    private static readonly string[] LexerInputs =
    [
        "select `t`.`id` AS `id`,`t`.`name` AS `name` from `db`.`t` where (`t`.`status` = 'active')",
        "select 'it''s', 'a\\'b', 'back\\\\slash', '', 'a' 'b', 'trailing\\\\'",
        "select \"x\"\"y\", \"a\\\"b\", \"\", \"multi\nline\"",
        "(_utf8mb4'text' collate utf8mb4_bin) = _latin1'x' or _notacharset",
        "description + active + reference + old_value + log_date + url + offset + auto_increment",
        "descriptioz + activz + referencz + ol_value + lo_date + ur",
        "MASTER_HOST = 'h', master_log_file, source_host, master, master_compression_algorithm",
        "grammar_selector_derived, parallel, qualify, tablesample bernoulli, s3, log(2), offset1, url_",
        "select /*!80000 1 */ + /*!99999 2 */ + /*! 3 */ + /* comment */ 4 -- trailing\n + # hash\n 5",
        "select $tag$ body $tag$, $$ other $$",
        "select 'unterminated",
        "select \"unterminated",
    ];

    // Lexes with the shared ATN, whose constant predicates have been removed, and compares the tokens with
    // those of a lexer running on a freshly deserialized, unmodified ATN.
    [TestCaseSource(nameof(LexerInputs))]
    public static void GetSignificantTokensSafe_GivenInput_ReturnsSameTokensAsUnmodifiedLexer(string sql)
    {
        var expected = LexWithUnmodifiedAtn(sql).Select(Describe).ToList();

        var actual = MySqlLexing.GetSignificantTokensSafe(sql).Select(Describe).ToList();

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public static void GetSignificantTokens_WhenLexerAtnPatched_LeavesOnlyStateDependentPredicates()
    {
        _ = MySqlLexing.GetSignificantTokens("select 1");

        var predicateRules = MySQLLexer._ATN.states
            .Where(static s => s != null)
            .SelectMany(static s => Enumerable.Range(0, s.NumberOfTransitions).Select(s.Transition))
            .OfType<PredicateTransition>()
            .Select(static t => MySQLLexer.ruleNames[t.ruleIndex])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal);

        Assert.That(predicateRules, Is.EqualTo(new[] { "DOLLAR_QUOTED_STRING_TEXT", "VERSION_COMMENT_END", "VERSION_COMMENT_START" }));
    }

    private static List<IToken> LexWithUnmodifiedAtn(string sql)
    {
        var lexer = new UnmodifiedAtnLexer(new AntlrInputStream(sql))
        {
            charSets = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "_utf8mb4", "_latin1" },
        };
        lexer.RemoveErrorListeners();

        var tokenStream = new CommonTokenStream(lexer);
        tokenStream.Fill();

        return tokenStream.GetTokens()
            .Where(static t => t.Channel == Lexer.DefaultTokenChannel && t.Type != TokenConstants.EOF)
            .ToList();
    }

    private static string Describe(IToken token) => $"{token.Type}:{token.StartIndex}-{token.StopIndex}:{token.Text}";

    private sealed class UnmodifiedAtnLexer : MySQLLexer
    {
        public UnmodifiedAtnLexer(ICharStream input)
            : base(input)
        {
            // Its own ATN and DFA cache, so it neither sees nor populates the shared, patched ones.
            var atn = new ATNDeserializer().Deserialize(SerializedAtn);
            var decisionToDfa = Enumerable.Range(0, atn.NumberOfDecisions)
                .Select(i => new DFA(atn.GetDecisionState(i), i))
                .ToArray();
            Interpreter = new LexerATNSimulator(this, atn, decisionToDfa, new PredictionContextCache());
        }
    }
}
