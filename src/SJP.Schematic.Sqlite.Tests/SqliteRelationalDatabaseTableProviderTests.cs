using System;
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
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabaseTableProvider(null, pragma, dialect, identifierDefaults, typeProvider));
        }

        [Test]
        public static void Ctor_GivenNullPragma_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabaseTableProvider(connection, null, dialect, identifierDefaults, typeProvider));
        }

        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabaseTableProvider(connection, pragma, null, identifierDefaults, typeProvider));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabaseTableProvider(connection, pragma, dialect, null, typeProvider));
        }

        [Test]
        public static void Ctor_GivenNullTypeProvider_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabaseTableProvider(connection, pragma, dialect, identifierDefaults, null));
        }

        [Test]
        public static void GetTable_GivenNullTableName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            var tableProvider = new SqliteRelationalDatabaseTableProvider(connection, pragma, dialect, identifierDefaults, typeProvider);

            Assert.Throws<ArgumentNullException>(() => tableProvider.GetTable(null));
        }

        [Test]
        public static void GetTableAsync_GivenNullTableName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            var tableProvider = new SqliteRelationalDatabaseTableProvider(connection, pragma, dialect, identifierDefaults, typeProvider);

            Assert.Throws<ArgumentNullException>(() => tableProvider.GetTableAsync(null));
        }
    }
}
