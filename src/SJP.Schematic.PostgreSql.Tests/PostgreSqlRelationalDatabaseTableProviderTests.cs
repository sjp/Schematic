using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlRelationalDatabaseTableProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.That(() => new PostgreSqlRelationalDatabaseTableProvider(null, identifierDefaults, identifierResolver, typeProvider), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.That(() => new PostgreSqlRelationalDatabaseTableProvider(connection, null, identifierResolver, typeProvider), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.That(() => new PostgreSqlRelationalDatabaseTableProvider(connection, identifierDefaults, null, typeProvider), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullTypeProvider_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

            Assert.That(() => new PostgreSqlRelationalDatabaseTableProvider(connection, identifierDefaults, identifierResolver, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetTable_GivenNullTableName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            var tableProvider = new PostgreSqlRelationalDatabaseTableProvider(connection, identifierDefaults, identifierResolver, typeProvider);

            Assert.That(() => tableProvider.GetTable(null), Throws.ArgumentNullException);
        }
    }
}
