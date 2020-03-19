using NUnit.Framework;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerDialectTests
    {
        [Test]
        public static void CreateConnectionAsync_GivenNull_ThrowsArgumentNullException()
        {
            Assert.That(() => SqlServerDialect.CreateConnectionAsync(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void CreateConnectionAsync_GivenEmptyString_ThrowsArgumentNullException()
        {
            Assert.That(() => SqlServerDialect.CreateConnectionAsync(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void CreateConnectionAsync_GivenWhiteSpaceString_ThrowsArgumentNullException()
        {
            Assert.That(() => SqlServerDialect.CreateConnectionAsync("   "), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuoteIdentifier_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new SqlServerDialect();

            Assert.That(() => dialect.QuoteIdentifier(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuoteIdentifier_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new SqlServerDialect();

            Assert.That(() => dialect.QuoteIdentifier(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuoteIdentifier_GivenWhiteSpace_ThrowsArgumentNullException()
        {
            var dialect = new SqlServerDialect();

            Assert.That(() => dialect.QuoteIdentifier("    "), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuoteName_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new SqlServerDialect();

            Assert.That(() => dialect.QuoteName(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuoteName_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new SqlServerDialect();

            Assert.That(() => dialect.QuoteName(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuoteName_GivenWhiteSpace_ThrowsArgumentNullException()
        {
            var dialect = new SqlServerDialect();

            Assert.That(() => dialect.QuoteName("    "), Throws.ArgumentNullException);
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
