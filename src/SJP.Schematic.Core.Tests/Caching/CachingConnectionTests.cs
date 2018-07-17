using NUnit.Framework;
using Moq;
using System;
using SJP.Schematic.Core.Caching;
using System.Data;

namespace SJP.Schematic.Core.Tests.Caching
{
    [TestFixture]
    internal static class CachingConnectionTests
    {
        private static Mock<IDbConnection> ConnectionMock => new Mock<IDbConnection>();

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CachingConnection(null));
        }

        [Test]
        public static void Ctor_GivenNullCacheStore_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CachingConnection(new DbConnectionAdapter(ConnectionMock.Object), null));
        }

        [Test]
        public static void Ctor_GivenNullConnectionAndNullCacheStore_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CachingConnection(null, null));
        }

        [Test]
        public static void ConnectionString_PropertyGet_ReadsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var cachingConnection = new CachingConnection(adapter);
            var connectionString = cachingConnection.ConnectionString;

            mock.VerifyGet(c => c.ConnectionString);
        }

        [Test]
        public static void ConnectionString_PropertySet_SetsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var cachingConnection = new CachingConnection(adapter);
            const string testConnectionString = "asd";
            cachingConnection.ConnectionString = testConnectionString;

            mock.VerifySet(c => c.ConnectionString = testConnectionString);
        }

        [Test]
        public static void Database_PropertyGet_ReadsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var cachingConnection = new CachingConnection(adapter);
            var database = cachingConnection.Database;

            mock.VerifyGet(c => c.Database);
        }

        [Test]
        public static void DataSource_PropertyGet_AlwaysEmptyString()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var cachingConnection = new CachingConnection(adapter);
            var connectionString = cachingConnection.ConnectionString;

            Assert.AreEqual(string.Empty, cachingConnection.DataSource);
        }

        [Test]
        public static void ServerVersion_PropertyGet_AlwaysEmptyString()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var cachingConnection = new CachingConnection(adapter);

            Assert.AreEqual(string.Empty, cachingConnection.ServerVersion);
        }

        [Test]
        public static void State_PropertyGet_ReadsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var cachingConnection = new CachingConnection(adapter);
            var state = cachingConnection.State;

            mock.VerifyGet(c => c.State);
        }

        [Test]
        public static void ChangeDatabase_WhenInvoked_CallsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var cachingConnection = new CachingConnection(adapter);
            const string databaseName = "asd";
            cachingConnection.ChangeDatabase(databaseName);

            mock.Verify(c => c.ChangeDatabase(databaseName));
        }

        [Test]
        public static void Close_WhenInvoked_CallsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var cachingConnection = new CachingConnection(adapter);
            cachingConnection.Close();

            mock.Verify(c => c.Close());
        }

        [Test]
        public static void Open_WhenInvoked_CallsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var cachingConnection = new CachingConnection(adapter);
            cachingConnection.Open();

            mock.Verify(c => c.Open());
        }

        [Test]
        public static void BeginTransaction_WhenInvoked_CallsProvidedConnection()
        {
            var mock = ConnectionMock;
            var mockTransaction = Mock.Of<IDbTransaction>();
            mock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(mockTransaction);

            var adapter = new DbConnectionAdapter(mock.Object);
            var cachingConnection = new CachingConnection(adapter);
            const IsolationLevel isolationLevel = IsolationLevel.ReadCommitted;
            var transaction = cachingConnection.BeginTransaction(isolationLevel);

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
            var cachingConnection = new CachingConnection(adapter);
            var command = cachingConnection.CreateCommand();

            mock.Verify(c => c.CreateCommand());
        }
    }
}
