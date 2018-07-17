using NUnit.Framework;
using Moq;
using System;
using SJP.Schematic.Core.Caching;
using System.Data;

namespace SJP.Schematic.Core.Tests.Caching
{
    [TestFixture]
    internal static class DbConnectionAdapterTests
    {
        private static Mock<IDbConnection> ConnectionMock => new Mock<IDbConnection>();

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbConnectionAdapter(null));
        }

        [Test]
        public static void ConnectionString_PropertyGet_ReadsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var connectionString = adapter.ConnectionString;

            mock.VerifyGet(c => c.ConnectionString);
        }

        [Test]
        public static void ConnectionString_PropertySet_SetsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            const string testConnectionString = "asd";
            adapter.ConnectionString = testConnectionString;

            mock.VerifySet(c => c.ConnectionString = testConnectionString);
        }

        [Test]
        public static void Database_PropertyGet_ReadsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var database = adapter.Database;

            mock.VerifyGet(c => c.Database);
        }

        [Test]
        public static void DataSource_PropertyGet_AlwaysEmptyString()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var connectionString = adapter.ConnectionString;

            Assert.AreEqual(string.Empty, adapter.DataSource);
        }

        [Test]
        public static void ServerVersion_PropertyGet_AlwaysEmptyString()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);

            Assert.AreEqual(string.Empty, adapter.ServerVersion);
        }

        [Test]
        public static void State_PropertyGet_ReadsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var state = adapter.State;

            mock.VerifyGet(c => c.State);
        }

        [Test]
        public static void ChangeDatabase_WhenInvoked_CallsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            const string databaseName = "asd";
            adapter.ChangeDatabase(databaseName);

            mock.Verify(c => c.ChangeDatabase(databaseName));
        }

        [Test]
        public static void Close_WhenInvoked_CallsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            adapter.Close();

            mock.Verify(c => c.Close());
        }

        [Test]
        public static void Open_WhenInvoked_CallsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            adapter.Open();

            mock.Verify(c => c.Open());
        }

        [Test]
        public static void BeginTransaction_WhenInvoked_CallsProvidedConnection()
        {
            var mock = ConnectionMock;
            var mockTransaction = Mock.Of<IDbTransaction>();
            mock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(mockTransaction);

            var adapter = new DbConnectionAdapter(mock.Object);
            const IsolationLevel isolationLevel = IsolationLevel.ReadCommitted;
            var transaction = adapter.BeginTransaction(isolationLevel);

            mock.Verify(c => c.BeginTransaction(isolationLevel));
        }

        [Test]
        public static void CreateCommand_WhenInvoked_CallsProvidedConnection()
        {
            var mock = ConnectionMock;
            var mockCommand = new Mock<IDbCommand>();
            var mockParameters = new Mock<IDataParameterCollection>();
            mockCommand.Setup(c => c.Parameters).Returns(mockParameters.Object);
            mock.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

            var adapter = new DbConnectionAdapter(mock.Object);
            var command = adapter.CreateCommand();

            mock.Verify(c => c.CreateCommand());
        }
    }
}
