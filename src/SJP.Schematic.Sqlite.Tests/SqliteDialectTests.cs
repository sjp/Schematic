using System;
using System.Data;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteDialectTests
    {
        [Test]
        public static void CreateConnectionAsync_GivenNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDialect.CreateConnectionAsync(null));
        }

        [Test]
        public static void CreateConnectionAsync_GivenEmptyString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDialect.CreateConnectionAsync(string.Empty));
        }

        [Test]
        public static void CreateConnectionAsync_GivenWhiteSpaceString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDialect.CreateConnectionAsync("   "));
        }

        [Test]
        public static void QuoteIdentifier_GivenNull_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new SqliteDialect(connection);

            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(null));
        }

        [Test]
        public static void QuoteIdentifier_GivenEmptyString_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new SqliteDialect(connection);

            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(string.Empty));
        }

        [Test]
        public static void QuoteIdentifier_GivenWhiteSpcae_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new SqliteDialect(connection);

            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier("    "));
        }

        [Test]
        public static void QuoteName_GivenNull_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new SqliteDialect(connection);

            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(null));
        }

        [Test]
        public static void QuoteName_GivenEmptyString_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new SqliteDialect(connection);

            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(string.Empty));
        }

        [Test]
        public static void QuoteName_GivenWhiteSpace_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new SqliteDialect(connection);

            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName("    "));
        }
    }
}
