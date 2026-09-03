using System;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests;

[TestFixture]
internal static class SqliteDialectTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuoteIdentifier_GivenNullOrWhiteSpaceIdentifier_ThrowsArgumentException(string identifier)
    {
        var dialect = new SqliteDialect();

        Assert.That(() => dialect.QuoteIdentifier(identifier), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuoteName_GivenNullOrWhiteSpaceName_ThrowsArgumentException(string name)
    {
        var dialect = new SqliteDialect();

        Assert.That(() => dialect.QuoteName(name), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void Capabilities_PropertyGet_DescribesSqlite()
    {
        var capabilities = new SqliteDialect().Capabilities;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(capabilities.SupportsSchemas, Is.True);
            Assert.That(capabilities.SupportsSequences, Is.False);
            Assert.That(capabilities.SupportsSynonyms, Is.False);
            Assert.That(capabilities.SupportsRoutines, Is.False);
            Assert.That(capabilities.SupportsMaterializedViews, Is.False);
            Assert.That(capabilities.SupportsComments, Is.False);
            Assert.That(capabilities.SupportsDeferrableConstraints, Is.True);
            Assert.That(capabilities.SupportsFilteredIndexes, Is.True);
            Assert.That(capabilities.SupportsIncludedIndexColumns, Is.False);
            Assert.That(capabilities.SupportsComputedColumns, Is.True);
            Assert.That(capabilities.SupportsIdentityColumns, Is.True);
            Assert.That(capabilities.SupportedReferentialActions, Has.Count.EqualTo(5));
            Assert.That(capabilities.FromLessSelectSuffix, OptionIs.None);
            Assert.That(capabilities.MaxIdentifierLength, Is.EqualTo(int.MaxValue));
        }
    }
}
