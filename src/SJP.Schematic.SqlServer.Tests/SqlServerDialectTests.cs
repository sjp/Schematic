using NUnit.Framework;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerDialectTests
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void CreateConnectionAsync_GivenNullOrWhiteSpaceConnectionString_ThrowsArgumentNullException(string connectionString)
        {
            Assert.That(() => SqlServerDialect.CreateConnectionAsync(connectionString), Throws.ArgumentNullException);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void QuoteIdentifier_GivenNullOrWhiteSpaceIdentifier_ThrowsArgumentNullException(string identifier)
        {
            var dialect = new SqlServerDialect();

            Assert.That(() => dialect.QuoteIdentifier(identifier), Throws.ArgumentNullException);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void QuoteName_GivenNullOrWhiteSpaceName_ThrowsArgumentNullException(string name)
        {
            var dialect = new SqlServerDialect();

            Assert.That(() => dialect.QuoteName(name), Throws.ArgumentNullException);
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
    }
}
