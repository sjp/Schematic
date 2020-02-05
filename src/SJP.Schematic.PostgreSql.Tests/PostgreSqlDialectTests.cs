using System.Data;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlDialectTests
    {
        [Test]
        public static void CreateConnectionAsync_GivenNull_ThrowsArgumentNullException()
        {
            Assert.That(() => PostgreSqlDialect.CreateConnectionAsync(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void CreateConnectionAsync_GivenEmptyString_ThrowsArgumentNullException()
        {
            Assert.That(() => PostgreSqlDialect.CreateConnectionAsync(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void CreateConnectionAsync_GivenWhiteSpaceString_ThrowsArgumentNullException()
        {
            Assert.That(() => PostgreSqlDialect.CreateConnectionAsync("   "), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuoteIdentifier_GivenNull_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect(connection);

            Assert.That(() => dialect.QuoteIdentifier(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuoteIdentifier_GivenEmptyString_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect(connection);

            Assert.That(() => dialect.QuoteIdentifier(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuoteIdentifier_GivenWhiteSpace_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect(connection);

            Assert.That(() => dialect.QuoteIdentifier("    "), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuoteName_GivenNull_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect(connection);

            Assert.That(() => dialect.QuoteName(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuoteName_GivenEmptyString_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect(connection);

            Assert.That(() => dialect.QuoteName(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuoteName_GivenWhiteSpace_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect(connection);

            Assert.That(() => dialect.QuoteName("    "), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuoteIdentifier_GivenRegularLocalName_ReturnsQuotedIdentifier()
        {
            const string input = "test_table";
            const string expected = "\"test_table\"";

            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect(connection);

            var result = dialect.QuoteIdentifier(input);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void QuoteIdentifier_GivenNameWithWhitespace_ReturnsQuotedIdentifier()
        {
            const string input = "test table name";
            const string expected = "\"test table name\"";

            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect(connection);

            var result = dialect.QuoteIdentifier(input);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void QuoteIdentifier_GivenNameWithDotSeparators_ReturnsQuotedIdentifier()
        {
            const string input = "test.table.name";
            const string expected = "\"test.table.name\"";

            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect(connection);

            var result = dialect.QuoteIdentifier(input);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void QuoteIdentifier_GivenNameWithQuoteAtStart_ReturnsQuotedIdentifier()
        {
            const string input = "\"test_table";
            const string expected = "\"\"\"test_table\"";

            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect(connection);

            var result = dialect.QuoteIdentifier(input);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void QuoteIdentifier_GivenNameWithQuoteInMiddle_ReturnsQuotedIdentifier()
        {
            const string input = "test\"table";
            const string expected = "\"test\"\"table\"";

            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect(connection);

            var result = dialect.QuoteIdentifier(input);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void QuoteIdentifier_GivenNameWithQuoteAtStartAndEnd_ReturnsQuotedIdentifier()
        {
            const string input = "\"test\"table";
            const string expected = "\"\"\"test\"\"table\"";

            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect(connection);

            var result = dialect.QuoteIdentifier(input);

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
