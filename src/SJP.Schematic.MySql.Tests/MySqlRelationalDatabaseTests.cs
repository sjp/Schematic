using System;
using NUnit.Framework;
using Moq;
using System.Data;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal static class MySqlRelationalDatabaseTests
    {
        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IDatabaseIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabase(null, connection, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var identifierDefaults = Mock.Of<IDatabaseIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabase(new MySqlDialect(), null, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabase(new MySqlDialect(), connection, null));
        }
    }
}
