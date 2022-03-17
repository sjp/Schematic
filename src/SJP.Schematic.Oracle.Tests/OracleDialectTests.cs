using NUnit.Framework;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleDialectTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuoteIdentifier_GivenNullOrWhiteSpaceIdentifier_ThrowsArgumentNullException(string identifier)
    {
        var dialect = new OracleDialect();

        Assert.That(() => dialect.QuoteIdentifier(identifier), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuoteName_GivenNullOrWhiteSpaceName_ThrowsArgumentNullException(string name)
    {
        var dialect = new OracleDialect();

        Assert.That(() => dialect.QuoteName(name), Throws.ArgumentNullException);
    }

    [Test]
    public static void QuoteName_GivenEmptyString_ThrowsArgumentNullException()
    {
        var dialect = new OracleDialect();

        Assert.That(() => dialect.QuoteName(string.Empty), Throws.ArgumentNullException);
    }

    [Test]
    public static void QuoteName_GivenWhiteSpace_ThrowsArgumentNullException()
    {
        var dialect = new OracleDialect();

        Assert.That(() => dialect.QuoteName("    "), Throws.ArgumentNullException);
    }

    [Test]
    public static void QuoteIdentifier_GivenRegularLocalName_ReturnsQuotedIdentifier()
    {
        const string input = "test_table";
        const string expected = "\"test_table\"";

        var dialect = new OracleDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void QuoteIdentifier_GivenNameWithWhitespace_ReturnsQuotedIdentifier()
    {
        const string input = "test table name";
        const string expected = "\"test table name\"";

        var dialect = new OracleDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void QuoteIdentifier_GivenNameWithDotSeparators_ReturnsQuotedIdentifier()
    {
        const string input = "test.table.name";
        const string expected = "\"test.table.name\"";

        var dialect = new OracleDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void QuoteIdentifier_GivenNameWithDoubleQuote_ThrowsArgumentException()
    {
        const string input = "\"test_table";

        var dialect = new OracleDialect();

        Assert.That(() => dialect.QuoteIdentifier(input), Throws.ArgumentException);
    }

    [Test]
    public static void QuoteIdentifier_GivenNameNullCharacter_ThrowsArgumentException()
    {
        var input = new string(new[] { 't', 'e', '\0', 's', 't' });

        var dialect = new OracleDialect();

        Assert.That(() => dialect.QuoteIdentifier(input), Throws.ArgumentException);
    }
}
