using System;
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

            Assert.Throws<ArgumentNullException>(() => new MySqlDatabaseRoutineProvider(null, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new MySqlDatabaseRoutineProvider(connection, null));
        }

        [Test]
        public static void GetRoutine_GivenNullRoutineName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var routineProvider = new MySqlDatabaseRoutineProvider(connection, identifierDefaults);

            Assert.Throws<ArgumentNullException>(() => routineProvider.GetRoutine(null));
        }
    }
}
