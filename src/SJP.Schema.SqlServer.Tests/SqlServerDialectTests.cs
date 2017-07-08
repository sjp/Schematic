using System;
using NUnit.Framework;

namespace SJP.Schema.SqlServer.Tests
{
    [TestFixture]
    internal class SqlServerDialectTests
    {
        [Test]
        public void QuoteIdentifier_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new SqlServerDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(null));
        }

        [Test]
        public void QuoteIdentifier_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new SqlServerDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(string.Empty));
        }

        [Test]
        public void QuoteIdentifier_GivenWhiteSpace_ThrowsArgumentNullException()
        {
            var dialect = new SqlServerDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier("    "));
        }

        [Test]
        public void QuoteName_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new SqlServerDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(null));
        }

        [Test]
        public void QuoteName_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new SqlServerDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(string.Empty));
        }

        [Test]
        public void QuoteName_GivenWhiteSpace_ThrowsArgumentNullException()
        {
            var dialect = new SqlServerDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName("    "));
        }
    }
}
