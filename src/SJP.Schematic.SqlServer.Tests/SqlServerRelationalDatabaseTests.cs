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
            var identifierDefaults = Mock.Of<IDatabaseIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(null, connection, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var identifierDefaults = Mock.Of<IDatabaseIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(new SqlServerDialect(), null, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(new SqlServerDialect(), connection, null));
        }
    }
}
