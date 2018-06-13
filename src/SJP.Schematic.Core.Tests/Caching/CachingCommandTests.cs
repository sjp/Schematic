using NUnit.Framework;
using Moq;
using System;
using SJP.Schematic.Core.Caching;
using System.Data;
using System.Data.Common;

namespace SJP.Schematic.Core.Tests.Caching
{
    [TestFixture]
    internal class CachingCommandTests
    {
        protected static DbConnection Connection => new DbConnectionAdapter(Mock.Of<IDbConnection>());

        protected static Mock<IDbCommand> CommandMock
        {
            get
            {
                var mockParameters = new Mock<IDataParameterCollection>();
                var mockCommand = new Mock<IDbCommand>();
                mockCommand.Setup(c => c.Parameters).Returns(mockParameters.Object);
                return mockCommand;
            }
        }

        protected static DbCommand DbCommandMock => new DbCommandAdapter(Connection, CommandMock.Object);

        protected static Mock<ICacheStore<int, DataTable>> CacheMock => new Mock<ICacheStore<int, DataTable>>();

        [Test]
        public void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CachingCommand(null, CacheMock.Object));
        }

        [Test]
        public void Ctor_GivenNullCacheStore_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CachingCommand(DbCommandMock, null));
        }

        [Test]
        public void Ctor_GivenNullConnectionAndNullCacheStore_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CachingCommand(null, null));
        }

        [Test]
        public void CommandText_PropertyGet_ReadsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            var commandText = cachingCommand.CommandText;

            mock.VerifyGet(c => c.CommandText);
        }

        [Test]
        public void CommandText_PropertySet_SetsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            const string commandText = "asd";
            new CachingCommand(command, CacheMock.Object) { CommandText = commandText };

            mock.VerifySet(c => c.CommandText = commandText);
        }

        [Test]
        public void CommandTimeout_PropertyGet_ReadsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            var commandText = cachingCommand.CommandText;

            mock.VerifyGet(c => c.CommandText);
        }

        [Test]
        public void CommandTimeout_PropertySet_SetsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            const int commandTimeout = 123;
            var cachingCommand = new CachingCommand(command, CacheMock.Object)
            {
                CommandTimeout = commandTimeout
            };

            mock.VerifySet(c => c.CommandTimeout = commandTimeout);
        }

        [Test]
        public void CommandType_PropertyGet_ReadsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            var commandText = cachingCommand.CommandText;

            mock.VerifyGet(c => c.CommandText);
        }

        [Test]
        public void CommandType_PropertySet_SetsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            const CommandType commandType = CommandType.Text;
            var cachingCommand = new CachingCommand(command, CacheMock.Object)
            {
                CommandType = commandType
            };

            mock.VerifySet(c => c.CommandType = commandType);
        }

        [Test]
        public void UpdatedRowSource_PropertyGet_ReadsProvidedCommand()
        {
            var mock = CommandMock;
            var adapter = new DbCommandAdapter(Connection, mock.Object);
            var commandText = adapter.UpdatedRowSource;

            mock.VerifyGet(c => c.UpdatedRowSource);
        }

        [Test]
        public void UpdatedRowSource_PropertySet_SetsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            const UpdateRowSource updatedRowSource = UpdateRowSource.FirstReturnedRecord;
            var cachingCommand = new CachingCommand(command, CacheMock.Object)
            {
                UpdatedRowSource = updatedRowSource
            };

            mock.VerifySet(c => c.UpdatedRowSource = updatedRowSource);
        }

        [Test]
        public void Cancel_WhenInvoked_CallsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            cachingCommand.Cancel();

            mock.Verify(c => c.Cancel());
        }

        [Test]
        public void ExecuteNonQuery_WhenInvoked_CallsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            cachingCommand.ExecuteNonQuery();

            mock.Verify(c => c.ExecuteNonQuery());
        }

        [Test]
        public void Prepare_WhenInvoked_CallsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            cachingCommand.Prepare();

            mock.Verify(c => c.Prepare());
        }

        // TODO: Implement the following tests better. There is a lot of mocking which is not providing much benefit
        //
        /*[Test]
        public void ExecuteScalar_WhenInvoked_CallsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            cachingCommand.ExecuteScalar();

            mock.Verify(c => c.ExecuteScalar());
        }

        [Test]
        public void ExecuteScalar_WhenInvokedTwice_CallsProvidedCommandOnlyOnce()
        {
            var mock = CommandMock;
            mock.Setup(c => c.ExecuteScalar()).Returns(1);
            mock.Setup(c => c.ExecuteScalar()).Returns(2);

            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            var result = Convert.ToInt32(cachingCommand.ExecuteScalar());
            result = Convert.ToInt32(cachingCommand.ExecuteScalar());

            mock.Verify(c => c.ExecuteScalar(), Times.Once);
        }

        [Test]
        public void ExecuteScalar_WhenInvokedTwice_ReturnsCachedResult()
        {
            var mock = CommandMock;
            mock.Setup(c => c.ExecuteScalar()).Returns(1);
            mock.Setup(c => c.ExecuteScalar()).Returns(2);

            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            var result = Convert.ToInt32(cachingCommand.ExecuteScalar());
            result = Convert.ToInt32(cachingCommand.ExecuteScalar());

            Assert.AreEqual(1, result);
        }

        [Test]
        public void ExecuteReader_WhenInvoked_CallsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            cachingCommand.ExecuteReader();

            mock.Verify(c => c.ExecuteReader());
        }

        [Test]
        public void ExecuteReader_WhenInvokedTwice_CallsProvidedCommandOnlyOnce()
        {
            var mock = CommandMock;
            var reader = Mock.Of<IDataReader>();
            mock.Setup(c => c.ExecuteReader()).Returns(reader);
            mock.Setup(c => c.ExecuteReader()).Returns(reader);

            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            var result = Convert.ToInt32(cachingCommand.ExecuteReader());
            result = Convert.ToInt32(cachingCommand.ExecuteReader());

            mock.Verify(c => c.ExecuteScalar(), Times.Once);
        }

        [Test]
        public void ExecuteReader_WhenInvoked_ReturnsCachedResult()
        {
            var mock = CommandMock;
            mock.Setup(c => c.ExecuteScalar()).Returns(1);
            mock.Setup(c => c.ExecuteScalar()).Returns(2);

            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            var result = Convert.ToInt32(cachingCommand.ExecuteScalar());
            result = Convert.ToInt32(cachingCommand.ExecuteScalar());

            Assert.AreEqual(1, result);
        }*/
    }
}
