using System;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleDialectTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuoteIdentifier_GivenNullOrWhiteSpaceIdentifier_ThrowsArgumentException(string identifier)
    {
        var dialect = new OracleDialect();

        Assert.That(() => dialect.QuoteIdentifier(identifier), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuoteName_GivenNullOrWhiteSpaceName_ThrowsArgumentException(string name)
    {
        var dialect = new OracleDialect();

        Assert.That(() => dialect.QuoteName(name), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void QuoteName_GivenEmptyString_ThrowsArgumentException()
    {
        var dialect = new OracleDialect();

        Assert.That(() => dialect.QuoteName(string.Empty), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void QuoteName_GivenWhiteSpace_ThrowsArgumentException()
    {
        var dialect = new OracleDialect();

        Assert.That(() => dialect.QuoteName("    "), Throws.InstanceOf<ArgumentException>());
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
        var input = new string(['t', 'e', '\0', 's', 't']);

        var dialect = new OracleDialect();

        Assert.That(() => dialect.QuoteIdentifier(input), Throws.ArgumentException);
    }

    [TestCase("SELECT")]
    [TestCase("select")]
    [TestCase("Select")]
    public static void IsReservedKeyword_GivenKeywordInAnyCasing_ReturnsTrue(string text)
    {
        var dialect = new OracleDialect();

        var result = dialect.IsReservedKeyword(text);

        Assert.That(result, Is.True);
    }

    [Test]
    public static void IsReservedKeyword_GivenNonKeyword_ReturnsFalse()
    {
        var dialect = new OracleDialect();

        var result = dialect.IsReservedKeyword("not_a_keyword_at_all");

        Assert.That(result, Is.False);
    }

    [Test]
    public static void Capabilities_PropertyGet_DescribesOracle()
    {
        var capabilities = new OracleDialect().Capabilities;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(capabilities.SupportsSchemas, Is.True);
            Assert.That(capabilities.SupportsSequences, Is.True);
            Assert.That(capabilities.SupportsSynonyms, Is.True);
            Assert.That(capabilities.SupportsRoutines, Is.True);
            Assert.That(capabilities.SupportsMaterializedViews, Is.True);
            Assert.That(capabilities.SupportsComments, Is.True);
            Assert.That(capabilities.SupportsDeferrableConstraints, Is.True);
            Assert.That(capabilities.SupportsFilteredIndexes, Is.False);
            Assert.That(capabilities.SupportsIncludedIndexColumns, Is.False);
            Assert.That(capabilities.SupportsComputedColumns, Is.True);
            Assert.That(capabilities.SupportsIdentityColumns, Is.True);
            Assert.That(capabilities.SupportedReferentialActions, Is.EquivalentTo(new[] { ReferentialAction.NoAction, ReferentialAction.Cascade, ReferentialAction.SetNull }));
            Assert.That(capabilities.FromLessSelectSuffix.UnwrapSome(), Is.EqualTo("DUAL"));
            Assert.That(capabilities.MaxIdentifierLength, Is.EqualTo(128));
        }
    }
}
