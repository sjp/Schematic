using System;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests;

[TestFixture]
internal static class PostgreSqlDialectTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuoteIdentifier_GivenNullOrWhiteSpaceIdentifier_ThrowsArgumentException(string identifier)
    {
        var dialect = new PostgreSqlDialect();

        Assert.That(() => dialect.QuoteIdentifier(identifier), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuoteName_GivenNullOrWhiteSpaceName_ThrowsArgumentException(string name)
    {
        var dialect = new PostgreSqlDialect();

        Assert.That(() => dialect.QuoteName(name), Throws.InstanceOf<ArgumentException>());
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

    [Test]
    public static void Capabilities_PropertyGet_DescribesPostgreSql()
    {
        var capabilities = new PostgreSqlDialect().Capabilities;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(capabilities.SupportsSchemas, Is.True);
            Assert.That(capabilities.SupportsSequences, Is.True);
            Assert.That(capabilities.SupportsSynonyms, Is.False);
            Assert.That(capabilities.SupportsRoutines, Is.True);
            Assert.That(capabilities.SupportsMaterializedViews, Is.True);
            Assert.That(capabilities.SupportsComments, Is.True);
            Assert.That(capabilities.SupportsDeferrableConstraints, Is.True);
            Assert.That(capabilities.SupportsFilteredIndexes, Is.True);
            Assert.That(capabilities.SupportsIncludedIndexColumns, Is.True);
            Assert.That(capabilities.SupportsComputedColumns, Is.True);
            Assert.That(capabilities.SupportsIdentityColumns, Is.True);
            Assert.That(capabilities.SupportedReferentialActions, Has.Count.EqualTo(5));
            Assert.That(capabilities.FromLessSelectSuffix, OptionIs.None);
            Assert.That(capabilities.MaxIdentifierLength, Is.EqualTo(63));
        }
    }
}
