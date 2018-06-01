using System;
using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal class SqliteDialectTests
    {
        [Test]
        public void CreateConnection_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnection(null));
        }

        [Test]
        public void CreateConnection_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnection(string.Empty));
        }

        [Test]
        public void CreateConnection_GivenWhiteSpaceString_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnection("   "));
        }

        [Test]
        public void CreateConnectionAsync_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnectionAsync(null));
        }

        [Test]
        public void CreateConnectionAsync_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnectionAsync(string.Empty));
        }

        [Test]
        public void CreateConnectionAsync_GivenWhiteSpaceString_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.CreateConnectionAsync("   "));
        }

        [Test]
        public void QuoteIdentifier_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(null));
        }

        [Test]
        public void QuoteIdentifier_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(string.Empty));
        }

        [Test]
        public void QuoteIdentifier_GivenWhiteSpcae_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier("    "));
        }

        [Test]
        public void QuoteName_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(null));
        }

        [Test]
        public void QuoteName_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(string.Empty));
        }

        [Test]
        public void QuoteName_GivenWhiteSpcae_ThrowsArgumentNullException()
        {
            var dialect = new SqliteDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName("    "));
        }
    }
}
