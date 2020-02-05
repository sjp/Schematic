using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteRelationalDatabaseTableProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqliteRelationalDatabaseTableProvider(null, pragma, dialect, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullPragma_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqliteRelationalDatabaseTableProvider(connection, null, dialect, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqliteRelationalDatabaseTableProvider(connection, pragma, null, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var dialect = Mock.Of<IDatabaseDialect>();

            Assert.That(() => new SqliteRelationalDatabaseTableProvider(connection, pragma, dialect, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetTable_GivenNullTableName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var tableProvider = new SqliteRelationalDatabaseTableProvider(connection, pragma, dialect, identifierDefaults);

            Assert.That(() => tableProvider.GetTable(null), Throws.ArgumentNullException);
        }
    }
}
