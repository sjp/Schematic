using System;
using System.Data;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleDialectTests
    {
        [Test]
        public static void CreateConnectionAsync_GivenNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => OracleDialect.CreateConnectionAsync(null));
        }

        [Test]
        public static void CreateConnectionAsync_GivenEmptyString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => OracleDialect.CreateConnectionAsync(string.Empty));
        }

        [Test]
        public static void CreateConnectionAsync_GivenWhiteSpaceString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => OracleDialect.CreateConnectionAsync("   "));
        }

        [Test]
        public static void QuoteIdentifier_GivenNull_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect(connection);

            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(null));
        }

        [Test]
        public static void QuoteIdentifier_GivenEmptyString_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect(connection);

            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(string.Empty));
        }

        [Test]
        public static void QuoteIdentifier_GivenWhiteSpace_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect(connection);

            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier("    "));
        }

        [Test]
        public static void QuoteName_GivenNull_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect(connection);

            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(null));
        }

        [Test]
        public static void QuoteName_GivenEmptyString_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect(connection);

            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(string.Empty));
        }

        [Test]
        public static void QuoteName_GivenWhiteSpace_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect(connection);

            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName("    "));
        }

        [Test]
        public static void QuoteIdentifier_GivenRegularLocalName_ReturnsQuotedIdentifier()
        {
            const string input = "test_table";
            const string expected = "\"test_table\"";

            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect(connection);

            var result = dialect.QuoteIdentifier(input);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void QuoteIdentifier_GivenNameWithWhitespace_ReturnsQuotedIdentifier()
        {
            const string input = "test table name";
            const string expected = "\"test table name\"";

            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect(connection);

            var result = dialect.QuoteIdentifier(input);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void QuoteIdentifier_GivenNameWithDotSeparators_ReturnsQuotedIdentifier()
        {
            const string input = "test.table.name";
            const string expected = "\"test.table.name\"";

            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect(connection);

            var result = dialect.QuoteIdentifier(input);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void QuoteIdentifier_GivenNameWithDoubleQuote_ThrowsArgumentException()
        {
            const string input = "\"test_table";

            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect(connection);

            Assert.Throws<ArgumentException>(() => dialect.QuoteIdentifier(input));
        }

        [Test]
        public static void QuoteIdentifier_GivenNameNullCharacter_ThrowsArgumentException()
        {
            var input = new string(new[] { 't', 'e', '\0', 's', 't' });

            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect(connection);

            Assert.Throws<ArgumentException>(() => dialect.QuoteIdentifier(input));
        }
    }
}
