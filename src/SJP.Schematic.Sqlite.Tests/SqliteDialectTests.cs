using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests;

[TestFixture]
internal static class SqliteDialectTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuoteIdentifier_GivenNullOrWhiteSpaceIdentifier_ThrowsArgumentNullException(string identifier)
    {
        var dialect = new SqliteDialect();

        Assert.That(() => dialect.QuoteIdentifier(identifier), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuoteName_GivenNullOrWhiteSpaceName_ThrowsArgumentNullException(string name)
    {
        var dialect = new SqliteDialect();

        Assert.That(() => dialect.QuoteName(name), Throws.ArgumentNullException);
    }
}