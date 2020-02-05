using NUnit.Framework;
using SJP.Schematic.Sqlite.Parsing;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Tests.Parsing
{
    [TestFixture]
    internal static class SqliteTableParserTests
    {
        [Test]
        public static void ParseTokens_GivenNullDefinition_ThrowsArgumentNullException()
        {
            var parser = new SqliteTableParser();
            var tokens = new TokenList<SqliteToken>(new[] { new Token<SqliteToken>(SqliteToken.Create, new TextSpan("CREATE")) });

            Assert.That(() => parser.ParseTokens(null, tokens), Throws.ArgumentNullException);
        }

        [Test]
        public static void ParseTokens_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            var parser = new SqliteTableParser();
            var tokens = new TokenList<SqliteToken>(new[] { new Token<SqliteToken>(SqliteToken.Create, new TextSpan("CREATE")) });

            Assert.That(() => parser.ParseTokens(string.Empty, tokens), Throws.ArgumentNullException);
        }

        [Test]
        public static void ParseTokens_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            var parser = new SqliteTableParser();
            var tokens = new TokenList<SqliteToken>(new[] { new Token<SqliteToken>(SqliteToken.Create, new TextSpan("CREATE")) });

            Assert.That(() => parser.ParseTokens("   ", tokens), Throws.ArgumentNullException);
        }

        [Test]
        public static void ParseTokens_GivenDefaultTokenList_ThrowsArgumentNullException()
        {
            var parser = new SqliteTableParser();

            Assert.That(() => parser.ParseTokens("TEST", default), Throws.ArgumentNullException);
        }
    }
}
