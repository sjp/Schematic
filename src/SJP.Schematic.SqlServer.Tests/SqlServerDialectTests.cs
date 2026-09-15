using System;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests;

internal static class SqlServerDialectTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuoteIdentifier_GivenNullOrWhiteSpaceIdentifier_ThrowsArgumentException(string identifier)
    {
        var dialect = new SqlServerDialect();

        Assert.That(
            () => dialect.QuoteIdentifier(identifier),
            Throws.InstanceOf<ArgumentException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifier")
        );
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuoteName_GivenNullOrWhiteSpaceName_ThrowsArgumentException(string name)
    {
        var dialect = new SqlServerDialect();

        Assert.That(
            () => dialect.QuoteName(name),
            Throws.InstanceOf<ArgumentException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("localName")
        );
    }

    [Test]
    public static void QuoteIdentifier_GivenRegularLocalName_ReturnsQuotedIdentifier()
    {
        const string input = "test_table";
        const string expected = "[test_table]";

        var dialect = new SqlServerDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void QuoteIdentifier_GivenNameWithWhitespace_ReturnsQuotedIdentifier()
    {
        const string input = "test table name";
        const string expected = "[test table name]";

        var dialect = new SqlServerDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void QuoteIdentifier_GivenNameWithDotSeparators_ReturnsQuotedIdentifier()
    {
        const string input = "test.table.name";
        const string expected = "[test.table.name]";

        var dialect = new SqlServerDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void QuoteIdentifier_GivenNameWithLeftSquareBracket_ReturnsQuotedIdentifier()
    {
        const string input = "[test_table";
        const string expected = "[[test_table]";

        var dialect = new SqlServerDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void QuoteIdentifier_GivenNameWithRightSquareBracket_ReturnsQuotedIdentifier()
    {
        const string input = "test]table";
        const string expected = "[test]]table]";

        var dialect = new SqlServerDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void QuoteIdentifier_GivenNameWithLeftAndRightSquareBrackets_ReturnsQuotedIdentifier()
    {
        const string input = "[test]table";
        const string expected = "[[test]]table]";

        var dialect = new SqlServerDialect();

        var result = dialect.QuoteIdentifier(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(null, null, null, "test_table", "[test_table]")]
    [TestCase(null, null, "test_schema", "test_table", "[test_schema].[test_table]")]
    [TestCase(null, "test_database", "test_schema", "test_table", "[test_database].[test_schema].[test_table]")]
    [TestCase("test_server", "test_database", "test_schema", "test_table", "[test_server].[test_database].[test_schema].[test_table]")]
    [TestCase("test]server", "test]database", "test]schema", "test]table", "[test]]server].[test]]database].[test]]schema].[test]]table]")]
    public static void QuoteName_GivenQualifiedName_QuotesEachComponent(string server, string database, string schema, string localName, string expected)
    {
        var name = Identifier.CreateQualifiedIdentifier(server, database, schema, localName);
        var dialect = new SqlServerDialect();

        var result = dialect.QuoteName(name);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void Capabilities_PropertyGet_DescribesSqlServer()
    {
        var capabilities = new SqlServerDialect().Capabilities;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(capabilities.SupportsSchemas, Is.True);
            Assert.That(capabilities.SupportsSequences, Is.True);
            Assert.That(capabilities.SupportsSynonyms, Is.True);
            Assert.That(capabilities.SupportsRoutines, Is.True);
            Assert.That(capabilities.SupportsMaterializedViews, Is.False);
            Assert.That(capabilities.SupportsComments, Is.True);
            Assert.That(capabilities.SupportsDeferrableConstraints, Is.False);
            Assert.That(capabilities.SupportsFilteredIndexes, Is.True);
            Assert.That(capabilities.SupportsIncludedIndexColumns, Is.True);
            Assert.That(capabilities.SupportsComputedColumns, Is.True);
            Assert.That(capabilities.SupportsIdentityColumns, Is.True);
            Assert.That(capabilities.SupportedReferentialActions, Does.Not.Contain(ReferentialAction.Restrict));
            Assert.That(capabilities.SupportedReferentialActions, Contains.Item(ReferentialAction.SetDefault));
            Assert.That(capabilities.FromLessSelectSuffix, OptionIs.None);
            Assert.That(capabilities.MaxIdentifierLength, Is.EqualTo(128));
        }
    }
}
