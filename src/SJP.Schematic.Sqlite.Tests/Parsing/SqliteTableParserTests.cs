using NUnit.Framework;
using SJP.Schematic.Sqlite.Parsing;

namespace SJP.Schematic.Sqlite.Tests.Parsing;

[TestFixture]
internal static class SqliteTableParserTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Parse_GivenNullOrWhiteSpaceDefinition_ThrowsArgumentException(string definition)
    {
        var parser = new SqliteTableParser();

        Assert.That(() => parser.Parse(definition), Throws.InstanceOf<System.ArgumentException>());
    }
}
