using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerDatabaseSequenceTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            const string sequenceName = "test_sequence";

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseSequence(null, database, sequenceName));
        }

        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            const string sequenceName = "test_sequence";

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseSequence(connection, null, sequenceName));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseSequence(connection, database, null));
        }

        [Test]
        public static void Database_PropertyGet_EqualsCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();
            const string sequenceName = "test_sequence";

            var sequence = new SqlServerDatabaseSequence(connection, database, sequenceName);

            Assert.AreEqual(database, sequence.Database);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();
            const string sequenceName = "test_sequence";

            var sequence = new SqlServerDatabaseSequence(connection, database, sequenceName);

            Assert.AreEqual(sequenceName, sequence.Name.LocalName);
        }
    }
}
