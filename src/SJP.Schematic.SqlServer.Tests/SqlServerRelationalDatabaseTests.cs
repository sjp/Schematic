using System;
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

            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(null, connection, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(new SqlServerDialect(), null, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(new SqlServerDialect(), connection, null));
        }

        [Test]
        public static void GetTable_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var database = new SqlServerRelationalDatabase(new SqlServerDialect(), connection, identifierDefaults);

            Assert.Throws<ArgumentNullException>(() => database.GetTable(null));
        }

        [Test]
        public static void GetView_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var database = new SqlServerRelationalDatabase(new SqlServerDialect(), connection, identifierDefaults);

            Assert.Throws<ArgumentNullException>(() => database.GetView(null));
        }

        [Test]
        public static void GetSequence_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var database = new SqlServerRelationalDatabase(new SqlServerDialect(), connection, identifierDefaults);

            Assert.Throws<ArgumentNullException>(() => database.GetSequence(null));
        }

        [Test]
        public static void GetSynonym_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var database = new SqlServerRelationalDatabase(new SqlServerDialect(), connection, identifierDefaults);

            Assert.Throws<ArgumentNullException>(() => database.GetSynonym(null));
        }
    }
}
