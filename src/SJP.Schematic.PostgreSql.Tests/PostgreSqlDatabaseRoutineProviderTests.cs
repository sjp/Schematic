using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlDatabaseRoutineProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

            Assert.That(() => new PostgreSqlDatabaseRoutineProvider(null, identifierDefaults, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

            Assert.That(() => new PostgreSqlDatabaseRoutineProvider(connection, null, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new PostgreSqlDatabaseRoutineProvider(connection, identifierDefaults, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetRoutine_GivenNullRoutineName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

            var routineProvider = new PostgreSqlDatabaseRoutineProvider(connection, identifierDefaults, identifierResolver);

            Assert.That(() => routineProvider.GetRoutine(null), Throws.ArgumentNullException);
        }
    }
}
