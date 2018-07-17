using System;
using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteDialectTests
    {
        [Test]
        public static void CreateConnection_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnection(null));
        }

        [Test]
        public static void CreateConnection_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnection(string.Empty));
        }

        [Test]
        public static void CreateConnection_GivenWhiteSpaceString_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnection("   "));
        }

        [Test]
        public static void CreateConnectionAsync_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnectionAsync(null));
        }

        [Test]
        public static void CreateConnectionAsync_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnectionAsync(string.Empty));
        }

        [Test]
        public static void CreateConnectionAsync_GivenWhiteSpaceString_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnectionAsync("   "));
        }

        [Test]
        public static void QuoteIdentifier_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(null));
        }

        [Test]
        public static void QuoteIdentifier_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(string.Empty));
        }

        [Test]
        public static void QuoteIdentifier_GivenWhiteSpcae_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier("    "));
        }

        [Test]
        public static void QuoteName_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(null));
        }

        [Test]
        public static void QuoteName_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(string.Empty));
        }

        [Test]
        public static void QuoteName_GivenWhiteSpcae_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName("    "));
        }
    }
}
