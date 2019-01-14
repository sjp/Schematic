using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerRelationalDatabaseRoutineProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseRoutineProvider(null, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseRoutineProvider(connection, null));
        }

        [Test]
        public static void GetRoutine_GivenNullRoutineName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var routineProvider = new SqlServerDatabaseRoutineProvider(connection, identifierDefaults);

            Assert.Throws<ArgumentNullException>(() => routineProvider.GetRoutine(null));
        }
    }
}
