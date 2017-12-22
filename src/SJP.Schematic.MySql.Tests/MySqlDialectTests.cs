using System;
using NUnit.Framework;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal class MySqlDialectTests
    {
        [Test]
        public void QuoteIdentifier_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new MySqlDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(null));
        }

        [Test]
        public void QuoteIdentifier_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new MySqlDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier(string.Empty));
        }

        [Test]
        public void QuoteIdentifier_GivenWhiteSpace_ThrowsArgumentNullException()
        {
            var dialect = new MySqlDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteIdentifier("    "));
        }

        [Test]
        public void QuoteName_GivenNull_ThrowsArgumentNullException()
        {
            var dialect = new MySqlDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(null));
        }

        [Test]
        public void QuoteName_GivenEmptyString_ThrowsArgumentNullException()
        {
            var dialect = new MySqlDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName(string.Empty));
        }

        [Test]
        public void QuoteName_GivenWhiteSpace_ThrowsArgumentNullException()
        {
            var dialect = new MySqlDialect();
            Assert.Throws<ArgumentNullException>(() => dialect.QuoteName("    "));
        }

        [Test]
        public void QuoteIdentifier_GivenRegularLocalName_ReturnsQuotedIdentifier()
        {
            const string input = "test_table";
            const string expected = "`test_table`";

            var dialect = new MySqlDialect();
            var result = dialect.QuoteIdentifier(input);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void QuoteIdentifier_GivenNameWithWhitespace_ReturnsQuotedIdentifier()
        {
            const string input = "test table name";
            const string expected = "`test table name`";

            var dialect = new MySqlDialect();
            var result = dialect.QuoteIdentifier(input);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void QuoteIdentifier_GivenNameWithDotSeparators_ReturnsQuotedIdentifier()
        {
            const string input = "test.table.name";
            const string expected = "`test.table.name`";

            var dialect = new MySqlDialect();
            var result = dialect.QuoteIdentifier(input);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void QuoteIdentifier_GivenNameWithBacktick_ReturnsQuotedIdentifier()
        {
            const string input = "test`table";
            const string expected = "`test``table`";

            var dialect = new MySqlDialect();
            var result = dialect.QuoteIdentifier(input);

            Assert.AreEqual(expected, result);
        }
    }
}
