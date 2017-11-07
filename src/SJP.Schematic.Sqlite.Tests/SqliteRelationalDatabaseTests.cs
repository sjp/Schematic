using System;
using NUnit.Framework;
using Moq;
using System.Data;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal class SqliteRelationalDatabaseTests
    {
        [Test]
        public void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabase(null, connection));
        }

        [Test]
        public void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabase(new SqliteDialect(), null));
        }

        [Test]
        public void Ctor_GivenNullDefaultSchema_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabase(new SqliteDialect(), connection, null));
        }

        [Test]
        public void Ctor_GivenEmptyDefaultSchema_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabase(new SqliteDialect(), connection, string.Empty));
        }

        [Test]
        public void Ctor_GivenWhiteSpaceDefaultSchema_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabase(new SqliteDialect(), connection, "   "));
        }

        [Test]
        public void DefaultSchema_GivenNoDefaultSchemaInCtor_EqualsMain()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);
            const string expectedDefaultSchema = "main";

            Assert.AreEqual(expectedDefaultSchema, database.DefaultSchema);
        }
    }
}
