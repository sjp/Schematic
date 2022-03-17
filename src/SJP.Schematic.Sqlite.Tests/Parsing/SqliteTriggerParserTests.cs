﻿using NUnit.Framework;
using SJP.Schematic.Sqlite.Parsing;

namespace SJP.Schematic.Sqlite.Tests.Parsing;

[TestFixture]
internal static class SqliteTriggerParserTests
{
    [Test]
    public static void ParseTokens_GivenDefaultTokenList_ThrowsArgumentNullException()
    {
        var parser = new SqliteTriggerParser();

        Assert.That(() => parser.ParseTokens(default), Throws.ArgumentNullException);
    }
}
