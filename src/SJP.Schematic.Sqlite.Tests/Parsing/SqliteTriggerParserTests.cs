using System;
using NUnit.Framework;
using SJP.Schematic.Sqlite.Parsing;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Tests.Parsing
{
    [TestFixture]
    internal static class SqliteTriggerParserTests
    {
        [Test]
        public static void ParseTokens_GivenDefaultTokenList_ThrowsArgumentNullException()
        {
            var parser = new SqliteTriggerParser();

            Assert.Throws<ArgumentNullException>(() => parser.ParseTokens(default(TokenList<SqliteToken>)));
        }
    }
}
