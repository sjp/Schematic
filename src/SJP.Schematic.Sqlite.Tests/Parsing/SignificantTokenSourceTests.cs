using System;
using System.Linq;
using Antlr4.Runtime;
using NUnit.Framework;
using SJP.Schematic.Sqlite.Parsing.Antlr;

namespace SJP.Schematic.Sqlite.Tests.Parsing;

internal static class SignificantTokenSourceTests
{
    [Test]
    public static void Ctor_GivenNullLexer_ThrowsArgumentNullException()
    {
        Assert.That(() => new SignificantTokenSource(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenLexer_BecomesLexerTokenFactory()
    {
        var lexer = new SQLiteLexer(new AntlrInputStream("select 1"));
        var tokenSource = new SignificantTokenSource(lexer);

        Assert.That(lexer.TokenFactory, Is.SameAs(tokenSource));
    }

    [TestCase("select 1")]
    [TestCase("  select\t1  ")]
    [TestCase("create table t (\r\n    id integer primary key, -- the key\r\n    name text /* a\nblock */ not null\r\n)")]
    [TestCase("create view v as select a.b, [c] from main.\"t\" as a where x = 'it''s'\n")]
    [TestCase("-- only a comment")]
    [TestCase("")]
    public static void NextToken_GivenSql_ReturnsDefaultChannelTokensOfUnwrappedLexer(string sql)
    {
        var expectedStream = new CommonTokenStream(new SQLiteLexer(new AntlrInputStream(sql)));
        expectedStream.Fill();
        var expected = expectedStream.GetTokens()
            .Where(static t => t.Channel == Lexer.DefaultTokenChannel)
            .Select(Describe)
            .ToList();

        var tokenSource = new SignificantTokenSource(new SQLiteLexer(new AntlrInputStream(sql)));
        var actual = new System.Collections.Generic.List<string>();
        IToken token;
        do
        {
            token = tokenSource.NextToken();
            actual.Add(Describe(token));
        }
        while (token.Type != TokenConstants.EOF);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public static void NextToken_GivenWhitespaceBetweenTokens_ReturnsDistinctTokenInstances()
    {
        var tokenSource = new SignificantTokenSource(new SQLiteLexer(new AntlrInputStream("select a , b")));

        var tokens = Enumerable.Range(0, 5).Select(_ => tokenSource.NextToken()).ToList();

        Assert.That(tokens, Is.Unique);
    }

    [Test]
    public static void TokenFactory_WhenSet_ThrowsNotSupportedException()
    {
        var tokenSource = new SignificantTokenSource(new SQLiteLexer(new AntlrInputStream("select 1")));

        Assert.That(() => tokenSource.TokenFactory = CommonTokenFactory.Default, Throws.TypeOf<NotSupportedException>());
    }

    private static string Describe(IToken token)
        => $"{token.Type}|{token.Text}|{token.Channel}|{token.StartIndex}|{token.StopIndex}|{token.Line}|{token.Column}";
}
