using NUnit.Framework;
using Moq;
using System;
using SJP.Schematic.Core.Caching;
using System.Data;
using System.Data.Common;

namespace SJP.Schematic.Core.Tests.Caching
{
    [TestFixture]
    internal static class CachingCommandTests
    {
        private static DbConnection Connection => new DbConnectionAdapter(Mock.Of<IDbConnection>());

        private static Mock<IDbCommand> CommandMock
        {
            get
            {
                var mockParameters = new Mock<IDataParameterCollection>();
                var mockCommand = new Mock<IDbCommand>();
                mockCommand.Setup(c => c.Parameters).Returns(mockParameters.Object);
                return mockCommand;
            }
        }

        private static DbCommand DbCommandMock => new DbCommandAdapter(Connection, CommandMock.Object);

        private static Mock<ICacheStore<int, DataTable>> CacheMock => new Mock<ICacheStore<int, DataTable>>();

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CachingCommand(null, CacheMock.Object));
        }

        [Test]
        public static void Ctor_GivenNullCacheStore_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CachingCommand(DbCommandMock, null));
        }

        [Test]
        public static void Ctor_GivenNullConnectionAndNullCacheStore_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CachingCommand(null, null));
        }

        [Test]
        public static void CommandText_PropertyGet_ReadsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            var commandText = cachingCommand.CommandText;

            mock.VerifyGet(c => c.CommandText);
        }

        [Test]
        public static void CommandText_PropertySet_SetsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            const string commandText = "asd";
            new CachingCommand(command, CacheMock.Object) { CommandText = commandText };

            mock.VerifySet(c => c.CommandText = commandText);
        }

        [Test]
        public static void CommandTimeout_PropertyGet_ReadsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            var commandTimeout = cachingCommand.CommandTimeout;

            mock.VerifyGet(c => c.CommandTimeout);
        }

        [Test]
        public static void CommandTimeout_PropertySet_SetsProvidedCommand()
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
        public static void CommandType_PropertyGet_ReadsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            var commandType = cachingCommand.CommandType;

            mock.VerifyGet(c => c.CommandType);
        }

        [Test]
        public static void CommandType_PropertySet_SetsProvidedCommand()
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
        public static void UpdatedRowSource_PropertyGet_ReadsProvidedCommand()
        {
            var mock = CommandMock;
            var adapter = new DbCommandAdapter(Connection, mock.Object);
            var commandText = adapter.UpdatedRowSource;

            mock.VerifyGet(c => c.UpdatedRowSource);
        }

        [Test]
        public static void UpdatedRowSource_PropertySet_SetsProvidedCommand()
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
        public static void Cancel_WhenInvoked_CallsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            cachingCommand.Cancel();

            mock.Verify(c => c.Cancel());
        }

        [Test]
        public static void ExecuteNonQuery_WhenInvoked_CallsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            cachingCommand.ExecuteNonQuery();

            mock.Verify(c => c.ExecuteNonQuery());
        }

        [Test]
        public static void Prepare_WhenInvoked_CallsProvidedCommand()
        {
            var mock = CommandMock;
            var command = new DbCommandAdapter(Connection, mock.Object);
            var cachingCommand = new CachingCommand(command, CacheMock.Object);
            cachingCommand.Prepare();

            mock.Verify(c => c.Prepare());
        }
    }
}
