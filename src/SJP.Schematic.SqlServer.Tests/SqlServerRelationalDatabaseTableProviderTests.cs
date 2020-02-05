using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerRelationalDatabaseTableProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.That(() => new SqlServerRelationalDatabaseTableProvider(null, identifierDefaults, typeProvider), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.That(() => new SqlServerRelationalDatabaseTableProvider(connection, null, typeProvider), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullTypeProvider_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqlServerRelationalDatabaseTableProvider(connection, identifierDefaults, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetTable_GivenNullTableName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            var tableProvider = new SqlServerRelationalDatabaseTableProvider(connection, identifierDefaults, typeProvider);

            Assert.That(() => tableProvider.GetTable(null), Throws.ArgumentNullException);
        }
    }
}
