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

        [Test]
        public void Vacuum_GivenNullSchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.Vacuum(null));
        }

        [Test]
        public void Vacuum_GivenEmptySchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.Vacuum(string.Empty));
        }

        [Test]
        public void Vacuum_GivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.Vacuum("   "));
        }

        [Test]
        public void VacuumAsync_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.VacuumAsync(null));
        }

        [Test]
        public void VacuumAsync_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.VacuumAsync(string.Empty));
        }

        [Test]
        public void VacuumAsync_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.VacuumAsync("   "));
        }

        [Test]
        public void AttachDatabase_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.AttachDatabase(null, ":memory:"));
        }

        [Test]
        public void AttachDatabase_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.AttachDatabase(string.Empty, ":memory:"));
        }

        [Test]
        public void AttachDatabase_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.AttachDatabase("   ", ":memory:"));
        }

        [Test]
        public void AttachDatabase_WhenGivenNullFileName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.AttachDatabase("test", null));
        }

        [Test]
        public void AttachDatabase_WhenGivenEmptyFileName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.AttachDatabase("test", string.Empty));
        }

        [Test]
        public void AttachDatabase_WhenGivenWhiteSpaceFileName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.AttachDatabase("test", "   "));
        }

        [Test]
        public void AttachDatabaseAsync_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.AttachDatabaseAsync(null, ":memory:"));
        }

        [Test]
        public void AttachDatabaseAsync_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.AttachDatabaseAsync(string.Empty, ":memory:"));
        }

        [Test]
        public void AttachDatabaseAsync_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.AttachDatabaseAsync("   ", ":memory:"));
        }

        [Test]
        public void AttachDatabaseAsync_WhenGivenNullFileName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.AttachDatabaseAsync("test", null));
        }

        [Test]
        public void AttachDatabaseAsync_WhenGivenEmptyFileName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.AttachDatabaseAsync("test", string.Empty));
        }

        [Test]
        public void AttachDatabaseAsync_WhenGivenWhiteSpaceFileName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.AttachDatabaseAsync("test", "   "));
        }

        [Test]
        public void DetachDatabase_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.DetachDatabase(null));
        }

        [Test]
        public void DetachDatabase_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.DetachDatabase(string.Empty));
        }

        [Test]
        public void DetachDatabase_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.DetachDatabase("   "));
        }

        [Test]
        public void DetachDatabaseAsync_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.DetachDatabaseAsync(null));
        }

        [Test]
        public void DetachDatabaseAsync_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.DetachDatabaseAsync(string.Empty));
        }

        [Test]
        public void DetachDatabaseAsync_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = new SqliteRelationalDatabase(new SqliteDialect(), connection);

            Assert.Throws<ArgumentNullException>(() => database.DetachDatabaseAsync("   "));
        }
    }
}
