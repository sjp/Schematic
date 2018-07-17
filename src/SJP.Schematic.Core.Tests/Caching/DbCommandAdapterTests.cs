using NUnit.Framework;
using Moq;
using System;
using SJP.Schematic.Core.Caching;
using System.Data;
using System.Data.Common;

namespace SJP.Schematic.Core.Tests.Caching
{
    [TestFixture]
    internal static class DbCommandAdapterTests
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

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbCommandAdapter(null, CommandMock.Object));
        }

        [Test]
        public static void Ctor_GivenNullCommand_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbCommandAdapter(Connection, null));
        }

        [Test]
        public static void Ctor_GivenNullConnectionAndNullCommand_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbCommandAdapter(null, null));
        }

        [Test]
        public static void DbConnection_PropertyGet_ReadsProvidedCommand()
        {
            var mock = CommandMock;
            var adapter = new FakeDbCommandAdapter(Connection, mock.Object);
            var connection = adapter.InnerConnection;

            mock.VerifyGet(c => c.Connection);
        }

        [Test]
        public static void DbConnection_PropertySet_SetsProvidedCommand()
        {
            var connection = Connection;
            var mock = CommandMock;
            var adapter = new FakeDbCommandAdapter(connection, mock.Object)
            {
                InnerConnection = connection
            };

            mock.VerifySet(c => c.Connection = connection);
        }

        [Test]
        public static void DbTransaction_PropertyGet_ReadsProvidedCommand()
        {
            var mock = CommandMock;
            var adapter = new FakeDbCommandAdapter(Connection, mock.Object);
            var transaction = adapter.InnerTransaction;

            mock.VerifyGet(c => c.Transaction);
        }

        [Test]
        public static void DbTransaction_PropertySet_SetsProvidedCommand()
        {
            var connection = Connection;
            var mock = CommandMock;
            DbTransaction transaction = null;
            var adapter = new FakeDbCommandAdapter(connection, mock.Object)
            {
                InnerTransaction = transaction
            };

            mock.VerifySet(c => c.Transaction = transaction);
        }

        [Test]
        public static void CommandText_PropertyGet_ReadsProvidedCommand()
        {
            var mock = CommandMock;
            var adapter = new DbCommandAdapter(Connection, mock.Object);
            var commandText = adapter.CommandText;

            mock.VerifyGet(c => c.CommandText);
        }

        [Test]
        public static void CommandText_PropertySet_SetsProvidedCommand()
        {
            var connection = Connection;
            var mock = CommandMock;
            const string commandText = "asd";
            var adapter = new DbCommandAdapter(connection, mock.Object)
            {
                CommandText = commandText
            };

            mock.VerifySet(c => c.CommandText = commandText);
        }

        [Test]
        public static void CommandTimeout_PropertyGet_ReadsProvidedCommand()
        {
            var mock = CommandMock;
            var adapter = new DbCommandAdapter(Connection, mock.Object);
            var commandTimeout = adapter.CommandTimeout;

            mock.VerifyGet(c => c.CommandTimeout);
        }

        [Test]
        public static void CommandTimeout_PropertySet_SetsProvidedCommand()
        {
            var connection = Connection;
            var mock = CommandMock;
            const int commandTimeout = 123;
            var adapter = new DbCommandAdapter(connection, mock.Object)
            {
                CommandTimeout = commandTimeout
            };

            mock.VerifySet(c => c.CommandTimeout = commandTimeout);
        }

        [Test]
        public static void CommandType_PropertyGet_ReadsProvidedCommand()
        {
            var mock = CommandMock;
            var adapter = new DbCommandAdapter(Connection, mock.Object);
            var commandType = adapter.CommandType;

            mock.VerifyGet(c => c.CommandType);
        }

        [Test]
        public static void CommandType_PropertySet_SetsProvidedCommand()
        {
            var connection = Connection;
            var mock = CommandMock;
            const CommandType commandType = CommandType.Text;
            var adapter = new DbCommandAdapter(connection, mock.Object)
            {
                CommandType = commandType
            };

            mock.VerifySet(c => c.CommandType = commandType);
        }

        [Test]
        public static void DesignTimeVisible_PropertyGet_ReturnsFalse()
        {
            var mock = CommandMock;
            var adapter = new DbCommandAdapter(Connection, mock.Object);
            var isVisible = adapter.DesignTimeVisible;

            Assert.IsFalse(isVisible);
        }

        [Test]
        public static void DesignTimeVisible_PropertyGetAfterSettingToTrue_ReturnsFalse()
        {
            var mock = CommandMock;
            var adapter = new DbCommandAdapter(Connection, mock.Object)
            {
                DesignTimeVisible = true
            };
            var isVisible = adapter.DesignTimeVisible;

            Assert.IsFalse(isVisible);
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
            var connection = Connection;
            var mock = CommandMock;
            const UpdateRowSource updatedRowSource = UpdateRowSource.FirstReturnedRecord;
            var adapter = new DbCommandAdapter(connection, mock.Object)
            {
                UpdatedRowSource = updatedRowSource
            };

            mock.VerifySet(c => c.UpdatedRowSource = updatedRowSource);
        }

        [Test]
        public static void Cancel_WhenInvoked_CallsProvidedCommand()
        {
            var mock = CommandMock;
            var adapter = new DbCommandAdapter(Connection, mock.Object);
            adapter.Cancel();

            mock.Verify(c => c.Cancel());
        }

        [Test]
        public static void ExecuteNonQuery_WhenInvoked_CallsProvidedCommand()
        {
            var mock = CommandMock;
            var adapter = new DbCommandAdapter(Connection, mock.Object);
            adapter.ExecuteNonQuery();

            mock.Verify(c => c.ExecuteNonQuery());
        }

        [Test]
        public static void ExecuteScalar_WhenInvoked_CallsProvidedCommand()
        {
            var mock = CommandMock;
            var adapter = new DbCommandAdapter(Connection, mock.Object);
            adapter.ExecuteScalar();

            mock.Verify(c => c.ExecuteScalar());
        }

        [Test]
        public static void Prepare_WhenInvoked_CallsProvidedCommand()
        {
            var mock = CommandMock;
            var adapter = new DbCommandAdapter(Connection, mock.Object);
            adapter.Prepare();

            mock.Verify(c => c.Prepare());
        }

        [Test]
        public static void CreateParameter_WhenInvoked_CallsProvidedCommand()
        {
            var mock = CommandMock;
            var mockParameter = Mock.Of<IDbDataParameter>();
            mock.Setup(c => c.CreateParameter()).Returns(mockParameter);

            var adapter = new DbCommandAdapter(Connection, mock.Object);
            var parameter = adapter.CreateParameter();

            mock.Verify(c => c.CreateParameter());
        }

        [Test]
        public static void ExecuteReader_WhenInvoked_CallsProvidedCommand()
        {
            var mock = CommandMock;
            var mockReader = Mock.Of<IDataReader>();
            mock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader);

            var adapter = new DbCommandAdapter(Connection, mock.Object);
            const CommandBehavior behavior = CommandBehavior.Default;
            var reader = adapter.ExecuteReader(behavior);

            mock.Verify(c => c.ExecuteReader(behavior));
        }

        protected class FakeDbCommandAdapter : DbCommandAdapter
        {
            public FakeDbCommandAdapter(DbConnection connection, IDbCommand command)
                : base(connection, command)
            {
            }

            public DbConnection InnerConnection
            {
                get => base.DbConnection;
                set => base.DbConnection = value;
            }

            public DbTransaction InnerTransaction
            {
                get => base.DbTransaction;
                set => base.DbTransaction = value;
            }
        }
    }
}
