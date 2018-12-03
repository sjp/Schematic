using System;
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
            var identifierDefaults = Mock.Of<IDatabaseIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new FakeRelationalDatabase(dialect, connection, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            IDbConnection connection = null;
            var identifierDefaults = Mock.Of<IDatabaseIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new FakeRelationalDatabase(dialect, connection, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            IDatabaseIdentifierDefaults identifierDefaults = null;

            Assert.Throws<ArgumentNullException>(() => new FakeRelationalDatabase(dialect, connection, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNonNullArguments_CreatesSuccessfully()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IDatabaseIdentifierDefaults>();

            Assert.DoesNotThrow(() => new FakeRelationalDatabase(dialect, connection, identifierDefaults));
        }

        private sealed class FakeRelationalDatabase : RelationalDatabase
        {
            public FakeRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IDatabaseIdentifierDefaults identifierDefaults)
                : base(dialect, connection, identifierDefaults)
            {
            }
        }
    }
}
