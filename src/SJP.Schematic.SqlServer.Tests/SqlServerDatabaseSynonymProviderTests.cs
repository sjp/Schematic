using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerRelationalDatabaseSynonymProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseSynonymProvider(null, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseSynonymProvider(connection, null));
        }

        [Test]
        public static void GetSynonym_GivenNullSynonymName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var synonymProvider = new SqlServerDatabaseSynonymProvider(connection, identifierDefaults);

            Assert.Throws<ArgumentNullException>(() => synonymProvider.GetSynonym(null));
        }

        [Test]
        public static void GetSynonymAsync_GivenNullSynonymName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var synonymProvider = new SqlServerDatabaseSynonymProvider(connection, identifierDefaults);

            Assert.Throws<ArgumentNullException>(() => synonymProvider.GetSynonymAsync(null));
        }
    }
}
