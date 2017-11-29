using NUnit.Framework;
using Moq;
using System;
using SJP.Schematic.Core.Caching;
using System.Data;

namespace SJP.Schematic.Core.Tests.Caching
{
    [TestFixture]
    public class DbConnectionAdapterTests
    {
        protected Mock<IDbConnection> ConnectionMock => new Mock<IDbConnection>();

        [Test]
        public void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbConnectionAdapter(null));
        }

        [Test]
        public void ConnectionString_PropertyGet_ReadsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var connectionString = adapter.ConnectionString;

            mock.VerifyGet(c => c.ConnectionString);
        }

        [Test]
        public void ConnectionString_PropertySet_SetsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            const string testConnectionString = "asd";
            adapter.ConnectionString = testConnectionString;

            mock.VerifySet(c => c.ConnectionString = testConnectionString);
        }

        [Test]
        public void Database_PropertyGet_ReadsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var database = adapter.Database;

            mock.VerifyGet(c => c.Database);
        }

        [Test]
        public void DataSource_PropertyGet_AlwaysEmptyString()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var connectionString = adapter.ConnectionString;

            Assert.AreEqual(string.Empty, adapter.DataSource);
        }

        [Test]
        public void ServerVersion_PropertyGet_AlwaysEmptyString()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);

            Assert.AreEqual(string.Empty, adapter.ServerVersion);
        }

        [Test]
        public void State_PropertyGet_ReadsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            var state = adapter.State;

            mock.VerifyGet(c => c.State);
        }

        [Test]
        public void ChangeDatabase_WhenInvoked_CallsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            const string databaseName = "asd";
            adapter.ChangeDatabase(databaseName);

            mock.Verify(c => c.ChangeDatabase(databaseName));
        }

        [Test]
        public void Close_WhenInvoked_CallsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            adapter.Close();

            mock.Verify(c => c.Close());
        }

        [Test]
        public void Open_WhenInvoked_CallsProvidedConnection()
        {
            var mock = ConnectionMock;
            var adapter = new DbConnectionAdapter(mock.Object);
            adapter.Open();

            mock.Verify(c => c.Open());
        }

        [Test]
        public void BeginTransaction_WhenInvoked_CallsProvidedConnection()
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
        public void CreateCommand_WhenInvoked_CallsProvidedConnection()
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
