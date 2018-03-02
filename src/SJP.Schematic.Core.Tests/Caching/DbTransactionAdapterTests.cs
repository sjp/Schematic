using NUnit.Framework;
using Moq;
using System;
using SJP.Schematic.Core.Caching;
using System.Data;
using System.Data.Common;

namespace SJP.Schematic.Core.Tests.Caching
{
    [TestFixture]
    internal class DbTransactionAdapterTests
    {
        protected DbConnection Connection => new DbConnectionAdapter(Mock.Of<IDbConnection>());

        protected Mock<IDbTransaction> TransactionMock => new Mock<IDbTransaction>();

        [Test]
        public void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbTransactionAdapter(null, TransactionMock.Object));
        }

        [Test]
        public void Ctor_GivenNullTransaction_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbTransactionAdapter(Connection, null));
        }

        [Test]
        public void Ctor_GivenNullConnectionAndNullTransaction_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbTransactionAdapter(null, null));
        }

        [Test]
        public void IsolationLevel_PropertyGet_ReadsProvidedTransaction()
        {
            var mock = TransactionMock;
            var adapter = new DbTransactionAdapter(Connection, mock.Object);
            var isolationLevel = adapter.IsolationLevel;

            mock.VerifyGet(t => t.IsolationLevel);
        }

        [Test]
        public void Commit_WhenInvoked_CallsProvidedTransaction()
        {
            var mock = TransactionMock;
            var adapter = new DbTransactionAdapter(Connection, mock.Object);
            adapter.Commit();

            mock.Verify(t => t.Commit());
        }

        [Test]
        public void Rollback_WhenInvoked_CallsProvidedTransaction()
        {
            var mock = TransactionMock;
            var adapter = new DbTransactionAdapter(Connection, mock.Object);
            adapter.Rollback();

            mock.Verify(t => t.Rollback());
        }
    }
}
