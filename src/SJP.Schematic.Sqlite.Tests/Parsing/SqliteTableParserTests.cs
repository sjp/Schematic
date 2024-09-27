using System;
using NUnit.Framework;
using SJP.Schematic.Sqlite.Parsing;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Tests.Parsing;

[TestFixture]
internal static class SqliteTableParserTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void ParseTokens_GivenNullOrWhiteSpaceDefinition_ThrowsArgumentException(string definition)
    {
        var parser = new SqliteTableParser();
        var tokens = new TokenList<SqliteToken>([new Token<SqliteToken>(SqliteToken.Create, new TextSpan("CREATE"))]);

        Assert.That(() => parser.ParseTokens(definition, tokens), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void ParseTokens_GivenDefaultTokenList_ThrowsArgumentNullException()
    {
        var parser = new SqliteTableParser();

        Assert.That(() => parser.ParseTokens("TEST", default), Throws.ArgumentNullException);
    }
}