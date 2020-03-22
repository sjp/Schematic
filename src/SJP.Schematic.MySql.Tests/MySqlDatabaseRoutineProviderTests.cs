using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal static class MySqlDatabaseRoutineProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new MySqlDatabaseRoutineProvider(null, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();

            Assert.That(() => new MySqlDatabaseRoutineProvider(connection, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetRoutine_GivenNullRoutineName_ThrowsArgNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var routineProvider = new MySqlDatabaseRoutineProvider(connection, identifierDefaults);

            Assert.That(() => routineProvider.GetRoutine(null), Throws.ArgumentNullException);
        }
    }
}
