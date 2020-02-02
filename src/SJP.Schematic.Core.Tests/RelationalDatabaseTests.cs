using System.Data;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class RelationalDatabaseTests
    {
        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            IDatabaseDialect dialect = null;
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new FakeRelationalDatabase(dialect, connection, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            IDbConnection connection = null;
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new FakeRelationalDatabase(dialect, connection, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            IIdentifierDefaults identifierDefaults = null;

            Assert.That(() => new FakeRelationalDatabase(dialect, connection, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNonNullArguments_CreatesSuccessfully()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new FakeRelationalDatabase(dialect, connection, identifierDefaults), Throws.Nothing);
        }

        private sealed class FakeRelationalDatabase : RelationalDatabase
        {
            public FakeRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IIdentifierDefaults identifierDefaults)
                : base(dialect, connection, identifierDefaults)
            {
            }
        }
    }
}
