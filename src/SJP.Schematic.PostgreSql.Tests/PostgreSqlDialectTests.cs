using NUnit.Framework;

namespace SJP.Schematic.PostgreSql.Tests;

[TestFixture]
internal static class PostgreSqlDialectTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuoteIdentifier_GivenNullOrWhiteSpaceIdentifier_ThrowsArgumentNullException(string identifier)
    {
        var dialect = new PostgreSqlDialect();

        Assert.That(() => dialect.QuoteIdentifier(identifier), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuoteName_GivenNullOrWhiteSpaceName_ThrowsArgumentNullException(string name)
    {
        var dialect = new PostgreSqlDialect();

        Assert.That(() => dialect.QuoteName(name), Throws.ArgumentNullException);
    }

    [Test]
    public static void QuoteIdentifier_GivenRegularLocalName_ReturnsQuotedIdentifier()
    {
        const string input = "test_table";
        const string expected = "\"test_table\"";

        var dialect = new PostgreSqlDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void QuoteIdentifier_GivenNameWithWhitespace_ReturnsQuotedIdentifier()
    {
        const string input = "test table name";
        const string expected = "\"test table name\"";

        var dialect = new PostgreSqlDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void QuoteIdentifier_GivenNameWithDotSeparators_ReturnsQuotedIdentifier()
    {
        const string input = "test.table.name";
        const string expected = "\"test.table.name\"";

        var dialect = new PostgreSqlDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void QuoteIdentifier_GivenNameWithQuoteAtStart_ReturnsQuotedIdentifier()
    {
        const string input = "\"test_table";
        const string expected = "\"\"\"test_table\"";

        var dialect = new PostgreSqlDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void QuoteIdentifier_GivenNameWithQuoteInMiddle_ReturnsQuotedIdentifier()
    {
        const string input = "test\"table";
        const string expected = "\"test\"\"table\"";

        var dialect = new PostgreSqlDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void QuoteIdentifier_GivenNameWithQuoteAtStartAndEnd_ReturnsQuotedIdentifier()
    {
        const string input = "\"test\"table";
        const string expected = "\"\"\"test\"\"table\"";

        var dialect = new PostgreSqlDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }
}
