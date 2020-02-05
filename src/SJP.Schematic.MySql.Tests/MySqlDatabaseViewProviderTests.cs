using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal static class MySqlDatabaseViewProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.That(() => new MySqlDatabaseViewProvider(null, identifierDefaults, typeProvider), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.That(() => new MySqlDatabaseViewProvider(connection, null, typeProvider), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullTypeProvider_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new MySqlDatabaseViewProvider(connection, identifierDefaults, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetView_GivenNullViewName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            var viewProvider = new MySqlDatabaseViewProvider(connection, identifierDefaults, typeProvider);

            Assert.That(() => viewProvider.GetView(null), Throws.ArgumentNullException);
        }
    }
}
