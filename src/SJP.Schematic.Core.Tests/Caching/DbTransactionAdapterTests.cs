using NUnit.Framework;
using Moq;
using System;
using SJP.Schematic.Core.Caching;
using System.Data;
using System.Data.Common;

namespace SJP.Schematic.Core.Tests.Caching
{
    [TestFixture]
    internal static class DbTransactionAdapterTests
    {
        private static DbConnection Connection => new DbConnectionAdapter(Mock.Of<IDbConnection>());

        private static Mock<IDbTransaction> TransactionMock => new Mock<IDbTransaction>();

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbTransactionAdapter(null, TransactionMock.Object));
        }

        [Test]
        public static void Ctor_GivenNullTransaction_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbTransactionAdapter(Connection, null));
        }

        [Test]
        public static void Ctor_GivenNullConnectionAndNullTransaction_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbTransactionAdapter(null, null));
        }

        [Test]
        public static void IsolationLevel_PropertyGet_ReadsProvidedTransaction()
        {
            var mock = TransactionMock;
            var adapter = new DbTransactionAdapter(Connection, mock.Object);
            var isolationLevel = adapter.IsolationLevel;

            mock.VerifyGet(t => t.IsolationLevel);
        }

        [Test]
        public static void Commit_WhenInvoked_CallsProvidedTransaction()
        {
            var mock = TransactionMock;
            var adapter = new DbTransactionAdapter(Connection, mock.Object);
            adapter.Commit();

            mock.Verify(t => t.Commit());
        }

        [Test]
        public static void Rollback_WhenInvoked_CallsProvidedTransaction()
        {
            var mock = TransactionMock;
            var adapter = new DbTransactionAdapter(Connection, mock.Object);
            adapter.Rollback();

            mock.Verify(t => t.Rollback());
        }
    }
}
