using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerDatabaseRoutineProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqlServerDatabaseRoutineProvider(null, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnectionFactory>();

            Assert.That(() => new SqlServerDatabaseRoutineProvider(connection, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetRoutine_GivenNullRoutineName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnectionFactory>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var routineProvider = new SqlServerDatabaseRoutineProvider(connection, identifierDefaults);

            Assert.That(() => routineProvider.GetRoutine(null), Throws.ArgumentNullException);
        }
    }
}
