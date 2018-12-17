using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteDatabaseViewProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new SqliteDatabaseViewProvider(null, pragma, dialect, identifierDefaults, typeProvider));
        }

        [Test]
        public static void Ctor_GivenNullPragma_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new SqliteDatabaseViewProvider(connection, null, dialect, identifierDefaults, typeProvider));
        }

        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new SqliteDatabaseViewProvider(connection, pragma, null, identifierDefaults, typeProvider));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new SqliteDatabaseViewProvider(connection, pragma, dialect, null, typeProvider));
        }

        [Test]
        public static void Ctor_GivenNullTypeProvider_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new SqliteDatabaseViewProvider(connection, pragma, dialect, identifierDefaults, null));
        }

        [Test]
        public static void GetView_GivenNullViewName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            var viewProvider = new SqliteDatabaseViewProvider(connection, pragma, dialect, identifierDefaults, typeProvider);

            Assert.Throws<ArgumentNullException>(() => viewProvider.GetView(null));
        }
    }
}
