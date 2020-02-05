using NUnit.Framework;
using Moq;
using System.Data;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerRelationalDatabaseTests
    {
        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqlServerRelationalDatabase(null, connection, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new SqlServerDialect(connection);
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqlServerRelationalDatabase(dialect, null, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new SqlServerDialect(connection);

            Assert.That(() => new SqlServerRelationalDatabase(dialect, connection, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetTable_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new SqlServerDialect(connection);
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var database = new SqlServerRelationalDatabase(dialect, connection, identifierDefaults);

            Assert.That(() => database.GetTable(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetView_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new SqlServerDialect(connection);
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var database = new SqlServerRelationalDatabase(dialect, connection, identifierDefaults);

            Assert.That(() => database.GetView(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSequence_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new SqlServerDialect(connection);
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var database = new SqlServerRelationalDatabase(dialect, connection, identifierDefaults);

            Assert.That(() => database.GetSequence(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSynonym_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new SqlServerDialect(connection);
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var database = new SqlServerRelationalDatabase(dialect, connection, identifierDefaults);

            Assert.That(() => database.GetSynonym(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetRoutine_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new SqlServerDialect(connection);
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var database = new SqlServerRelationalDatabase(dialect, connection, identifierDefaults);

            Assert.That(() => database.GetRoutine(null), Throws.ArgumentNullException);
        }
    }
}
