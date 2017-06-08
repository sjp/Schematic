using System;
using NUnit.Framework;

namespace SJP.Schema.SQLite.Tests
{
    [TestFixture]
    public class SqlServerDialectTests
    {
        [Test]
        public void QuotingIdentifierThrowsOnNullInput()
        {
            var dialect = SQLiteDialect.Instance;
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(null));
        }

        [Test]
        public void QuotingIdentifierThrowsOnEmptyInput()
        {
            var dialect = SQLiteDialect.Instance;
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(string.Empty));
        }

        [Test]
        public void QuotingIdentifierThrowsOnWhiteSpaceInput()
        {
            var dialect = SQLiteDialect.Instance;
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier("    "));
        }

        [Test]
        public void QuotingNameThrowsOnNullInput()
        {
            var dialect = SQLiteDialect.Instance;
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(null));
        }

        [Test]
        public void QuotingNameThrowsOnEmptyInput()
        {
            var dialect = SQLiteDialect.Instance;
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(string.Empty));
        }

        [Test]
        public void QuotingNameThrowsOnWhiteSpaceInput()
        {
            var dialect = SQLiteDialect.Instance;
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName("    "));
        }
    }
}
