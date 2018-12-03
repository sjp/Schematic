using System;
using NUnit.Framework;
using Moq;
using System.Data;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteRelationalDatabaseTests
    {
        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabase(null, connection, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabase(new SqliteDialect(), null, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabase(new SqliteDialect(), connection, null));
        }

        private static ISqliteDatabase Database
        {
            get
            {
                var dialect = new SqliteDialect();
                var connection = Mock.Of<IDbConnection>();
                var identifierDefaults = Mock.Of<IIdentifierDefaults>();

                return new SqliteRelationalDatabase(dialect, connection, identifierDefaults);
            }
        }

        [Test]
        public static void Vacuum_GivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.Vacuum(null));
        }

        [Test]
        public static void Vacuum_GivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.Vacuum(string.Empty));
        }

        [Test]
        public static void Vacuum_GivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.Vacuum("   "));
        }

        [Test]
        public static void VacuumAsync_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.VacuumAsync(null));
        }

        [Test]
        public static void VacuumAsync_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.VacuumAsync(string.Empty));
        }

        [Test]
        public static void VacuumAsync_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.VacuumAsync("   "));
        }

        [Test]
        public static void AttachDatabase_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.AttachDatabase(null, ":memory:"));
        }

        [Test]
        public static void AttachDatabase_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.AttachDatabase(string.Empty, ":memory:"));
        }

        [Test]
        public static void AttachDatabase_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.AttachDatabase("   ", ":memory:"));
        }

        [Test]
        public static void AttachDatabase_WhenGivenNullFileName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.AttachDatabase("test", null));
        }

        [Test]
        public static void AttachDatabase_WhenGivenEmptyFileName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.AttachDatabase("test", string.Empty));
        }

        [Test]
        public static void AttachDatabase_WhenGivenWhiteSpaceFileName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.AttachDatabase("test", "   "));
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.AttachDatabaseAsync(null, ":memory:"));
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.AttachDatabaseAsync(string.Empty, ":memory:"));
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.AttachDatabaseAsync("   ", ":memory:"));
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenNullFileName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.AttachDatabaseAsync("test", null));
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenEmptyFileName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.AttachDatabaseAsync("test", string.Empty));
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenWhiteSpaceFileName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.AttachDatabaseAsync("test", "   "));
        }

        [Test]
        public static void DetachDatabase_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.DetachDatabase(null));
        }

        [Test]
        public static void DetachDatabase_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.DetachDatabase(string.Empty));
        }

        [Test]
        public static void DetachDatabase_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.DetachDatabase("   "));
        }

        [Test]
        public static void DetachDatabaseAsync_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.DetachDatabaseAsync(null));
        }

        [Test]
        public static void DetachDatabaseAsync_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.DetachDatabaseAsync(string.Empty));
        }

        [Test]
        public static void DetachDatabaseAsync_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.DetachDatabaseAsync("   "));
        }
    }
}
