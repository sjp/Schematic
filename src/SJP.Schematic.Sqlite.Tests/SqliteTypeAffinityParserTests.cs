using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests;

internal static class SqliteTypeAffinityParserTests
{
    // TODO
    [Test]
    public static void Ctor_GivenNoComparers_CreatesWithoutError()
    {
        Assert.That(() => new SqliteTypeAffinityParser(), Throws.Nothing);
    }
}