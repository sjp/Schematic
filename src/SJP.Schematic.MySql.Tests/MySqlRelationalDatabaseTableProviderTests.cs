using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal static class MySqlRelationalDatabaseTableProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new MySqlRelationalDatabaseTableProvider(null, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();

            Assert.That(() => new MySqlRelationalDatabaseTableProvider(connection, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetTable_GivenNullTableName_ThrowsArgNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var tableProvider = new MySqlRelationalDatabaseTableProvider(connection, identifierDefaults);

            Assert.That(() => tableProvider.GetTable(null), Throws.ArgumentNullException);
        }
    }
}
