using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
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
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqliteDatabaseViewProvider(null, pragma, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullPragma_ThrowsArgNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqliteDatabaseViewProvider(connection, null, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();

            Assert.That(() => new SqliteDatabaseViewProvider(connection, pragma, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetView_GivenNullViewName_ThrowsArgNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var viewProvider = new SqliteDatabaseViewProvider(connection, pragma, identifierDefaults);

            Assert.That(() => viewProvider.GetView(null), Throws.ArgumentNullException);
        }
    }
}
