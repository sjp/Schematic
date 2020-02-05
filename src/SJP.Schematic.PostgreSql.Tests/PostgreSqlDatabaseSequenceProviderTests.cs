using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlDatabaseSequenceProviderTests
    {
        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

            Assert.That(() => new PostgreSqlDatabaseSequenceProvider(null, connection, identifierDefaults, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

            Assert.That(() => new PostgreSqlDatabaseSequenceProvider(dialect, null, identifierDefaults, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

            Assert.That(() => new PostgreSqlDatabaseSequenceProvider(dialect, connection, null, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new PostgreSqlDatabaseSequenceProvider(dialect, connection, identifierDefaults, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSequence_GivenNullSequenceName_ThrowsArgNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

            var sequenceProvider = new PostgreSqlDatabaseSequenceProvider(dialect, connection, identifierDefaults, identifierResolver);

            Assert.That(() => sequenceProvider.GetSequence(null), Throws.ArgumentNullException);
        }
    }
}
