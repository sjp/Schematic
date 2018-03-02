using System;
using System.Data;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal class RelationalDatabaseTests
    {
        [Test]
        public void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            IDatabaseDialect dialect = null;
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new FakeRelationalDatabase(dialect, connection));
        }

        [Test]
        public void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            IDbConnection connection = null;

            Assert.Throws<ArgumentNullException>(() => new FakeRelationalDatabase(dialect, connection));
        }

        [Test]
        public void Ctor_GivenNonNullArguments_CreatesSuccessfully()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();

            Assert.DoesNotThrow(() => new FakeRelationalDatabase(dialect, connection));
        }

        private class FakeRelationalDatabase : RelationalDatabase
        {
            public FakeRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection)
                : base(dialect, connection)
            {
            }
        }
    }
}
